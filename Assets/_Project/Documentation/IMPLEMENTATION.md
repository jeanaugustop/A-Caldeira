# Implementação do protótipo

## Estrutura e responsabilidades

```text
Assets/_Project/
  Scripts/
    Core/         GameManager, PoolManager, RuntimeServicesSO
    Data/         WeaponSO, EnemySO, UpgradeSO, StageSO, PoolKeySO
    Events/       canais de estado, início/fim e XP
    Pooling/      GenericObjectPool<T>, PooledBehaviour, IPoolable
    Simulation/   GameplaySimulation, SpatialGrid, PerformanceProbe
    Combat/       WeaponManager, ProjectileActor
    Enemies/      EnemyActor
    Collectibles/ CollectibleActor
    World/        WaveSpawner, HazardActor
    Input/        PlayerInputReader
    Progression/  RunProgression, PermanentProgression
    Audio/        GameAudio
    UI/           menus, HUD, oficina, settings
  Editor/         geração de conteúdo exclusivamente no Editor
  Tests/          testes EditMode e PlayMode
  Generated/      criado pelo gerador (cenas, dados, arte, áudio e prefabs)
Tools/Verification/ testes isolados executáveis com .NET 8
```

O gerador mantém todos os serviços, o jogador, bancos, câmera e UI em um único
root persistente. As cenas de fase contêm o cenário; o menu tem sua apresentação
no canvas persistente. Referências internas são serializadas. RuntimeServicesSO
é a ponte para os comandos de menu; não guarda dados de save.

## Comunicação implementada

```text
MainMenuManager --comando--> GameManager --estado--> GameStateEventChannelSO
                                |                         |-> GameplayUI
                                |                         `-> GameAudio
                                |-> limpa PoolManager
                                `-> Begin(GameplaySimulation)

GameplaySimulation --Tick--> WaveSpawner --rent--> PoolManager
        |-----------Tick--> WeaponManager --rent--> PoolManager
        |-----------dano--> EnemyActor --despawn--> PoolManager
        |                       `-> IEnemyLifecycleListener -> WaveSpawner
        |-----------morte (evento)--> GameAudio
        |-----------coleta--> RunProgression --XP (canal)--> GameplayUI
        |                           |--LevelUp--> GameManager
        |                           `--ChoicesChanged--> GameplayUI
        `-----------HUD (evento com valores)-----------> GameplayUI

GameplayUI --escolha--> RunProgression --equipar/evoluir--> WeaponManager
GameManager --fim de partida--> PermanentProgression --Changed--> Oficina/UI
SettingsPanel --comandos--> SettingsManager --Changed--> GameAudio
```

Canais publicam fatos; comandos diretos chamam o único responsável. Inscrições
da UI e áudio são balanceadas em OnEnable/OnDisable. Nenhum ator executa Update.

## Contratos de memória e ciclo de vida

- Pools copiam o array de cadastro, rejeitam nulos, duplicatas, tipos misturados
  e objetos já pertencentes a outro pool. A propriedade usa identidade do pool;
  compartilhar PoolKeySO não permite devolver um objeto para outro proprietário.
- Capacidade fixa e falha explícita. Tipo solicitado é verificado antes da ativação.
- Cadastros e arrays auxiliares alocam no carregamento. Nenhum crescimento no loop.
- ReturnAll é executado antes de carregar outra partida/menu, antes de zerar os
  contadores da nova onda. Saídas de gameplay param a simulação pelo estado.
- Estado é limpo ao devolver atores. Configure é chamado imediatamente após o
  aluguel; OnEnable de futuros componentes não deve consumir dados de Configure.
- Prefabs são materializados como objetos inativos pelo gerador, exclusivamente
  no Editor. Não há Instantiate/Destroy/AddComponent no código runtime.
- Grade fixa de 48x36 células com listas encadeadas em arrays; não existe limite
  artificial de ocupação por célula. Consultas usam distância real após a filtragem.
- Colisão de projétil usa segmento do frame para reduzir atravessamento. Histórico
  limitado de 16 impactos por projétil usa índice e geração do inimigo.
- Limite atual de perfuração: 16 impactos por projétil. Corpos inimigos não fazem
  colisão física entre si; empilhamento é permitido no protótipo.
- Carregamento, transições de UI, serialização de save e geração Editor podem
  alocar. GC zero é uma meta do estado estável de gameplay, a medir na engine.

## Performance e limites

Bancos gerados: 1.504 inimigos, 1.600 projéteis, 1.280 coletáveis e 32 hazards.
O teste sintético usa 1.200 inimigos em movimento orbital distribuído, sem morte,
para manter a carga. O jogador é invulnerável nesse teste; duração 65 segundos.
O relatório coleta até 7.200 frames após cinco segundos de aquecimento.

Não confundir ausência de alocações no código C# isolado com ausência de
alocações internas do Unity/TMP/Input System. O verificador .NET usa dublês de
Unity, sem execução de componentes nativos. Ainda é necessário:

1. Gerar o conteúdo e compilar no Unity 2022.3.
2. Executar testes EditMode/PlayMode.
3. Inspecionar layouts, seleção por controle, toque, mixer e mudança de vídeo.
4. Medir GC e frame time em builds PC e nos dispositivos mobile escolhidos.
5. Comparar cena sintética e partida real. Tratar 16,67 ms como orçamento de frame;
   p95, picos e GPU também contam, não apenas FPS médio.

O baseline é GameObjects/SpriteRenderer e arrays, sem Jobs/Burst. A grade reduz
consultas, mas regiões muito densas continuam custosas. Se o profiler indicar
gargalo de CPU/renderização, migrar o sistema medido para Jobs/Burst ou renderização
em lotes é o próximo trabalho, sem alterar os dados autorais.

## Persistência e configuração

Configurações: PlayerPrefs `acal.settings.v1` com JsonUtility. Progressão:
`progression-v1.json` com Newtonsoft JSON em persistentDataPath, gravação temporária
e backup da versão anterior. Saves ilegíveis retornam aos valores padrão.
Áudio usa três parâmetros expostos: MasterVolume, SfxVolume e MusicVolume.
A aplicação inicial do mixer ocorre em Start, conforme a documentação Unity.

As APIs e o gerador têm como baseline Unity 2022.3 / URP 14; versões principais
superiores podem exigir migração de packages, mixer serializado e APIs de Editor.

## Referências consultadas

- [AudioMixer.SetFloat e inicialização em Start](https://docs.unity.cn/2022.3/Documentation/ScriptReference/Audio.AudioMixer.SetFloat.html)
- [Implementação oficial de UniversalRenderPipelineAsset, branch 2022.3](https://github.com/Unity-Technologies/Graphics/blob/2022.3/staging/Packages/com.unity.render-pipelines.universal/Runtime/Data/UniversalRenderPipelineAsset.cs)
