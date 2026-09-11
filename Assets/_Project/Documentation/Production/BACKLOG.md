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

#### Evoluções de armas — efeitos aprovados; fusões pendentes

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
  - Duas armas no nível 6 ficam elegíveis para uma fusão. A Forja consome ambas,
    cria a evolução de nível 7 e libera um slot de equipamento.
  - Definir quais duplas de armas produzem cada evolução.
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
  - Direção atual: testar regeneração passiva de 1 de vida por segundo, em vez de
    cura ativa ou drop de cura. A vida máxima pode crescer 20 pontos a cada dez
    níveis do jogador; detalhes em `JOGADOR.md`.
- [ ] Projetar a progressão da run expandida: cerca de 20 minutos, ondas especiais
  como marcos e um chefão no minuto 20.
  - Esta é uma tarefa de Onda 2; não alterar a duração atual antes de definir
    inimigo-chefe, recompensas, ritmo e quantidade de upgrades.
  - A curva inicial de vida, quantidade e princípios de dano está registrada em
    `COMBATE.md` para orientar os playtests da versão expandida.
- [ ] Implementar aprimoramentos básicos repetíveis com raridade por carta.
  - Melhorias de atributo, como raio de coleta, não se esgotam; a repetição é uma
    escolha do jogador e define o risco da run.
  - Desbloqueios de armas e evoluções continuam únicos e saem das ofertas após
    serem obtidos.
  - Cada oferta recebe raridade em runtime: Comum, Incomum, Rara, Épica e
    Lendária. "Sorte" altera a chance de raridades altas.
  - A tabela de porcentagem por raridade é única para todos os atributos:
    Comum 4%, Incomum 7%, Rara 10%, Épica 13% e Lendária 16%. Por exemplo, um
    card Raro concede 10% a dano, velocidade, coleta ou cadência. Esta é a
    primeira versão para playtest.
  - Repetir um atributo não reduz o valor do card. Em vez disso, cada cópia já
    escolhida reduz o peso/chance daquele atributo voltar a aparecer, sem levá-lo
    a zero. Assim é possível focar uma build, mas fica progressivamente menos
    provável encontrar o mesmo card.
  - A tela nunca mostra "Aprimoramentos esgotados": o sistema sempre oferece
    opções básicas repetíveis quando o conteúdo único já foi obtido.
  - Toda oferta possui uma arma, um atributo geral e um acessório. O atributo
    recebe raridade; armas e acessórios seguem trilhas fixas sem raridade.
  - Enquanto houver slot vazio, a carta de arma ou acessório pode oferecer item
    novo ou o próximo nível de um item já equipado. Com os oito slots preenchidos,
    só oferece níveis dos equipamentos existentes. Dentro de cada categoria,
    todos os candidatos elegíveis possuem a mesma chance.
  - Sem candidato elegível de arma ou acessório, aquela posição é preenchida por
    um atributo geral; a tela nunca fica com menos de três escolhas.
  - O bônus dos atributos é sempre calculado sobre o valor-base. As regras e
    tabelas completas estão em `ATRIBUTOS.md`.
  - Válvula Rápida será apresentada como **Cadência de Disparo** e afetará todas
    as armas automáticas, inclusive evoluções e a recriação de armas orbitais.
  - A run terá oito slots compartilhados por armas e acessórios. Atributos gerais
    não ocupam slot; com os oito preenchidos, novos equipamentos deixam de sair
    das ofertas e permanecem apenas as melhorias dos itens já equipados e os
    atributos gerais.
  - Esquiva é um atributo geral com a mesma tabela de raridade e teto de 65% de
    chance total. Ao ativar, nega completamente um dano.
  - Toda run começa com três rerolls de ofertas; eles não ocupam slot. Um reroll
    troca as três opções de uma vez e é acionado por botão visível ou tecla `R`
    no PC.
- [ ] Implementar a primeira leva de acessórios únicos: Anel de Contingência,
  Fusível Sacrificial, Válvula de Pânico, Placa de Amortecimento, Bobina de
  Recolhimento, Sirene de Contenção e Cabo de Aterramento.
  - Cada acessório terá níveis 1 a 6 predeterminados; o nível 7 será uma evolução
    da Forja. As trilhas aprovadas estão centralizadas em `ACESSORIOS.md`.
- [ ] Definir o orçamento de progressão de uma run de 20 minutos antes de fixar
  nível máximo ou números de armas.
  - Meta inicial aprovada: cerca de nível 45, ou 44 escolhas após o nível inicial,
    ao minuto 20. Medir depois os marcos de 5, 10 e 15 minutos.
  - Não haverá cotas obrigatórias entre equipamentos, níveis de armas e atributos
    gerais: a distribuição depende das escolhas da build. Os playtests validarão
    a frequência com que cada tipo de card aparece.
  - Cada arma terá níveis predeterminados, em vez de uma escolha de atributo
    interno a cada nível. O jogador escolhe qual arma avançar para seu próximo
    efeito definido.
  - Cada arma possui sete níveis no total: os níveis 1 a 6 são a arma base e
    suas cinco melhorias; o nível 7 é uma evolução obtida pela fusão, na Forja,
    de duas armas no nível 6. As sequências de efeitos estão em `ARMAS.md` e
    todas as trilhas atuais estão aprovadas.

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
