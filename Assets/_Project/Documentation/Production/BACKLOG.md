# Backlog de produção

Use este arquivo como fonte única de tarefas. Mova itens entre as seções; não
duplique a mesma tarefa em outros lugares.

## Em andamento

- [ ] Preparar uma build de playtest do Pátio de Triagem.
- [ ] Convidar três pessoas que não participaram do desenvolvimento para testar.

## Feedback recebido — aguarda investigação

### Prioridade 0 — clareza do loop

- [ ] Tornar o fim de partida inequívoco: vitória e derrota precisam ter título,
  cor, causa e próximo passo diferentes.
  - Origem: a pessoa viu apenas "Turno concluído" e não soube se venceu ou morreu.
  - Texto aprovado para morte: **"VOCÊ MORREU"** / "O exotraje cedeu. A Caldeira não para."
  - Texto aprovado para conclusão de fase: **"TURNO CONCLUÍDO"** / "Você sobreviveu ao Pátio de Triagem. A próxima linha já está em movimento."
  - Texto aprovado para vitória contra chefão final: **"A LINHA FOI ROMPIDA"** / "O coração da máquina silenciou. Por enquanto."
  - Usar títulos fixos para deixar o resultado claro e uma frase narrativa aleatória
    abaixo. Criar bancos separados de cerca de 20 frases para morte e conclusão
    de fase; não repetir uma frase até esgotar o respectivo banco.
- [ ] Garantir três opções de aprimoramento em todo level-up.
  - Origem: o catálogo atual possui limites e hoje as escolhas podem desaparecer.
  - Aceite: quando armas e evoluções não forem elegíveis, o sistema preenche as
    ofertas com aprimoramentos básicos repetíveis.

### Prioridade 1 — controles e balanceamento

- [ ] Permitir `Espaço` e `Enter` para confirmar o aprimoramento selecionado.
- [ ] Identificar a arma considerada forte demais e ajustar dano, recarga, área ou
  condição de desbloqueio.
- [ ] Rebalancear o início da partida, pois o playtest classificou a dificuldade
  como muito fácil.

### Prioridade 2 — decisões de design

#### Evoluções de armas — propostas a validar

| Arma | Evolução proposta | Efeito pretendido |
|---|---|---|
| Rebites de Pressão | **Tempestade de Rebites** | Já existe no protótipo: transforma o tiro simples em disparo triplo, rápido e perfurante. |
| Óleo Cru | **Fornalha Ambulante** | O leque de óleo deixa rastros incendiários breves no chão, punindo grupos que perseguem o jogador. |
| Serras Orbitais | **Coroa de Moendas** | Cria uma segunda órbita em sentido contrário, ampliando a barreira ao redor do exotraje. |
| Estacas Hidráulicas | **Perfuração Sísmica** | As estacas que atravessam inimigos liberam uma onda de impacto no fim do percurso. |
| Prensa de Choque | **Linha de Montagem** | Três prensas atingem áreas em sequência. |
| Carga de Retardo | **Demolição em Cadeia** | A explosão deixa fogo/óleo no chão ou detona minas próximas em sequência. |

- [ ] Definir requisitos, efeitos visuais, raridade e números de cada evolução
  somente após testar as armas base.
- [ ] Implementar o sistema de forjas para evoluções de arma.
  - Uma arma no nível máximo fica elegível para evolução.
  - Cada mapa contém uma forja em posição e visual próprios; ela cria uma decisão
    de rota, risco e momento de uso durante a horda.
  - Ao chegar à forja e ativá-la, abrir uma tela de forja — não uma tela de
    carregamento — e pausar a simulação enquanto o jogador escolhe a evolução.
    O risco termina ao ativar a forja, não durante a leitura da escolha.
  - A ativação pode exigir proximidade/tempo curto ou um botão, a definir para
    PC e mobile.
  - Não usar missões baseadas em mira ou precisão, pois o combate é automático.
  - Decidir depois: custo em recursos, quando a forja é ativada, número máximo de
    evoluções por run e se elites/ondas especiais fornecem componentes.

- [ ] Adicionar **Prensa de Choque**.
  - Periodicamente atinge a área com a maior concentração de inimigos, causa dano
    em área e empurra sobreviventes.
  - Papel: controle de multidão e abertura de espaço.
  - Evolução proposta: **Linha de Montagem**, com três prensas atingindo áreas em
    sequência.
- [ ] Adicionar **Carga de Retardo**.
  - Solta uma mina na posição anterior do jogador; após alguns segundos, ela
    explode em área e empurra inimigos.
  - Papel: recompensar movimento e kiting da horda.
  - Evolução proposta: **Demolição em Cadeia**, deixando fogo/óleo no chão ou
    detonando minas próximas em sequência.
- [ ] Projetar uma fonte de cura: drop, upgrade, recompensa de chefe ou cura entre
  ondas. Definir custo, frequência e limites antes de implementar.
- [ ] Projetar a progressão da run expandida: cerca de 20 minutos, ondas especiais
  como marcos e um chefão no minuto 20.
  - Esta é uma tarefa de Onda 2; não alterar a duração atual antes de definir
    inimigo-chefe, recompensas, ritmo e quantidade de upgrades.
- [ ] Implementar aprimoramentos básicos repetíveis com raridade por carta.
  - Melhorias de atributo, como raio de coleta, não se esgotam; a repetição é uma
    escolha do jogador e define o risco da run.
  - Desbloqueios de armas e evoluções continuam únicos e saem das ofertas após
    serem obtidos.
  - Cada oferta recebe raridade em runtime: Comum, Incomum, Rara, Épica e
    Lendária. "Sorte" altera a chance de raridades altas.
  - Para raio de coleta, a referência inicial é +10%, +20%, +30%, +40% e +50%
    conforme a raridade. Estes valores são referência, não decisão final; discutir
    o balanceamento de cada atributo depois.
  - A tela nunca mostra "Aprimoramentos esgotados": o sistema sempre oferece
    opções básicas repetíveis quando o conteúdo único já foi obtido.

## Onda 1 — Polimento do Pátio

### Prioridade 0 — precisa funcionar para o playtest

- [ ] Observar o primeiro minuto de cada jogador sem dar instruções.
  - Aceite: o jogador entende movimento, disparo automático e coleta.
- [ ] Registrar onde cada jogador para, se confunde ou morre.
  - Aceite: três registros completos em `PLAYTESTS.md`.
- [ ] Corrigir os três problemas com maior impacto na clareza ou diversão.
  - Aceite: cada correção é testada novamente em Play Mode.
- [ ] Ajustar ritmo inicial de inimigos, XP e upgrades conforme o feedback.
  - Aceite: a primeira escolha de upgrade ocorre naturalmente e é compreensível.

### Prioridade 1 — melhora a sensação

- [ ] Revisar resposta do movimento do jogador e leitura de colisão.
- [ ] Reforçar feedback de tiro, acerto, dano e coleta com VFX/SFX provisórios.
- [ ] Melhorar legibilidade do HUD em resolução 1280x720.
- [ ] Revisar pausa, reinício e retorno ao menu após uma partida.

### Prioridade 2 — depois do núcleo

- [ ] Criar uma tela curta de instruções para a primeira partida.
- [ ] Definir referências de arte, paleta e tipografia industrial.
- [ ] Definir referências de música e efeitos sonoros.

## Fora do escopo agora

- Integração Steam ou publicação em loja.
- Multiplayer, monetização ou backend.
- Terceira fase, novos sistemas grandes ou migração para Jobs/Burst sem evidência
  do profiler.
