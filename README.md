# A Caldeira: Sobrevivência de Ferro

## Versão artística integrada

O projeto principal agora inclui a passagem visual dieselpunk: personagem e inimigos direcionais animados, armas e acessórios ilustrados, interface temática e o Pátio de Triagem ampliado para 180 × 120 unidades.

Use Unity **2022.3.62f3** e abra `Assets/_Project/Generated/Scenes/00_Bootstrap.unity`. Detalhes da direção visual estão em `Assets/_Project/Documentation/Production/VERSAO_COM_ARTES.md`.

Protótipo Unity 2022.3 LTS / URP 2D para PC e mobile. Inclui combate centralizado,
pools fixos, progressão durante a partida, oficina permanente e interface.

## Abrir e jogar

1. Adicione esta pasta no Unity Hub e abra com Unity **2022.3 LTS**.
2. Aguarde a resolução dos packages e compilação. O gerador tenta montar o protótipo na primeira importação.
3. Se necessário, salve a cena aberta e execute **Tools > A Caldeira > Generate Prototype**.
4. Abra `Assets/_Project/Generated/Scenes/00_Bootstrap.unity` e pressione Play.

O gerador cria cenas, assets de dados, prefabs, bancos de objetos inativos,
renderizador URP 2D, interface TMP, mixer Master/SFX/BGM, textura industrial e sons
provisórios. Não baixa arte. Um projeto já gerado não é sobrescrito pelo comando.
O Input System pode solicitar reinício do Editor após a configuração inicial.

- WASD/setas ou analógico esquerdo: movimento. Disparo automático.
- Esc/Start ou botão Pausa: pausar/continuar.
- Mobile: arrastar na metade esquerda da tela; botões à direita/centro para menus.
- Colete sucata/óleo, escolha melhorias e sobreviva por cinco minutos.
- A evolução da pistola exige a melhoria Pressão Extra.
- Derrotas/vitórias concedem créditos; abandonar pelo menu não concede recompensa.

## Conteúdo implementado

- Pátio de Triagem e Linhas de Montagem, com frequências e tempos de hazards diferentes.
- Drones, tratores-sucatadores e máquinas pesadas.
- Rebites, leque de óleo em combustão, serras orbitais e estacas perfurantes.
- Atração/coleta de XP, níveis, seleção de três ofertas sem duplicatas, pré-requisitos e evolução.
- Oficina persistente: blindagem e potência, com custos progressivos e limites.
- Menu, HUD, pausa, derrota/vitória, reinício e retorno ao menu.
- Áudio e vídeo persistentes; confirmação de vídeo com reversão após 12 segundos.
- Cena de carga com 1.200 inimigos e relatório de desempenho.

## Validação

No Unity: abra **Window > General > Test Runner** e execute EditMode e PlayMode
depois de gerar o protótipo. Os testes cobrem pool, grade e ciclo de partida.
O menu **Tools > A Caldeira > Validate Open Bootstrap** verifica as referências
serializadas; **Build Windows Development** gera `Builds/Windows/ACaldeira.exe`.

Sem Unity, com .NET SDK 8 instalado, execute na raiz:

```powershell
dotnet run --project Tools/Verification/Verification.csproj -- .
```

Esse verificador analisa a sintaxe C# e testa o código do pool e da grade, com
dublês mínimos de Unity. **Não comprova compilação contra as APIs Unity, renderização,
funcionamento das cenas ou GC da engine.**

Para medir: gere uma Development Build Windows, abra Teste de Carga / 1200,
mantenha a janela em foco e deixe terminar. O relatório `stress-report.txt` fica
em `Application.persistentDataPath`. Ele exclui os primeiros cinco segundos e
registra média/p95, GC, mínimo de inimigos e esgotamentos de pool. O teste é
sintético; também meça uma partida real com coleta, áudio e interface ativos.

**Estado de entrega:** código e gerador implementados; cenas e assets finais dependem
da execução do gerador no Unity. Não há executável ou medição de 60 FPS validada
neste ambiente, onde o Editor Unity não foi localizado.

Arte e áudio são provisórios. O protótipo não inclui integração Steamworks,
publicação em lojas, certificação de dispositivos ou conteúdo de produção.

Veja [a arquitetura atual](Assets/_Project/Documentation/IMPLEMENTATION.md) e
[a especificação inicial](Assets/_Project/Documentation/ARCHITECTURE.md).
