# Armas e evoluções

> **Disponibilidade no protótipo:** O **Óleo Cru** está temporariamente suspenso
> das escolhas da run. Seu leque atual desequilibra o combate; a arma será
> rebalanceada antes de voltar ao jogo.

## Regras da progressão

- Cada arma possui sete níveis no total. Os níveis 1 a 6 são a arma base e suas
  cinco melhorias predeterminadas; o nível 7 é sua evolução.
- Ao receber uma melhoria de arma, o jogador escolhe qual arma avançar para o
  próximo nível da sua trilha; não escolhe um atributo interno dela.
- Na Forja, duas armas no nível 6 serão fundidas em uma evolução de nível 7.
  A fusão consome as duas armas e libera um slot de equipamento.
- Armas e acessórios competem pelos oito slots compartilhados da run.

Os valores numéricos de dano, recarga, área e duração serão definidos em
playtests. Todas as trilhas abaixo estão aprovadas como direção de design. As
duplas exatas necessárias para cada fusão ainda serão definidas.

## Rebites de Pressão — arma primária versátil

Dispara rebites automaticamente. Começa confiável e se transforma em uma linha
de produção de projéteis.

1. **Rebites de Pressão** — dispara automaticamente um rebite.
2. **Bocal Duplo** — dispara `+1` rebite por ataque.
3. **Têmpera Industrial** — rebites atravessam `+1` inimigo.
4. **Carregador Rotativo** — melhora especificamente a cadência dos Rebites.
5. **Cabeça Expandida** — rebites maiores, com mais dano e área de impacto.
6. **Pistão de Impacto** — a cada alguns disparos, lança um rebite pesado que
   causa muito dano, atravessa vários inimigos e empurra a linha.

7. **Tempestade de Rebites** — evolução da Forja; substitui os tiros isolados
   por rajadas rápidas, múltiplas e perfurantes.

## Óleo Cru — controle de grupos próximos

Espalha um leque de óleo e mantém a horda sob pressão ao redor do jogador.

1. **Óleo Cru** — pulveriza gotículas de óleo em um leque de curto alcance.
2. **Bico Alargado** — adiciona mais gotículas a cada pulverização.
3. **Óleo Viscoso** — inimigos atingidos ficam desacelerados por um breve período.
4. **Mangueira Pressurizada** — aumenta o alcance e a velocidade do jato.
5. **Tanque de Refluxo** — as gotículas permanecem ativas por mais tempo e
   atravessam mais inimigos.
6. **Pulverizador de Alta Vazão** — a cada alguns disparos, libera um leque
   amplo de óleo reforçado.

7. **Fornalha Ambulante** — evolução da Forja; o óleo deixa rastros incendiários
   breves no chão.

## Serras Orbitais — defesa de perímetro

Serras circulam o exotraje e castigam inimigos que se aproximam demais.

1. **Serras Orbitais** — três serras orbitam o jogador e ferem inimigos no contato.
2. **Cubo de Engrenagem** — adiciona uma serra à órbita.
3. **Braço Extensor** — aumenta o raio da órbita.
4. **Correia Reforçada** — aumenta a velocidade de rotação.
5. **Dentes Temperados** — serras maiores causam mais dano e acertam uma área maior.
6. **Eixo Oscilante** — a órbita alterna entre posições mais próximas e mais
   distantes, varrendo uma faixa maior ao redor do jogador.

7. **Coroa de Moendas** — evolução da Forja; cria uma segunda órbita de serras
   girando no sentido contrário.

## Estacas Hidráulicas — dano e abertura de caminho em linha

Dispara estacas pesadas para atravessar fileiras de inimigos e abrir caminho.

1. **Estacas Hidráulicas** — dispara uma estaca pesada de longo alcance.
2. **Pistão Duplo** — dispara `+1` estaca por ativação.
3. **Ponta Perfurante** — cada estaca atravessa mais inimigos.
4. **Propulsor Hidráulico** — aumenta velocidade e alcance das estacas.
5. **Núcleo Denso** — estacas causam mais dano e empurram inimigos atingidos.
6. **Trilho de Cravação** — estacas maiores mantêm uma linha de dano mais longa
   durante o percurso.

7. **Perfuração Sísmica** — evolução da Forja; ao encerrar o percurso, as
   estacas liberam uma onda de impacto.

## Prensa de Choque — ataque em área contra concentrações

Identifica a maior horda e a comprime com uma prensa pesada.

1. **Prensa de Choque** — periodicamente atinge a região com mais inimigos,
   causando dano em área e empurrando sobreviventes.
2. **Ciclo Acelerado** — reduz o tempo entre as prensas.
3. **Matriz Ampla** — aumenta a área atingida.
4. **Contrapeso Reforçado** — aumenta dano e força de empurrão.
5. **Base Hidráulica** — a área comprimida desacelera inimigos por um breve período.
6. **Dupla Estampagem** — cada ativação recebe uma segunda prensada mais fraca
   no mesmo ponto.

7. **Linha de Montagem** — evolução da Forja; três prensas atingem regiões em
   sequência.

## Carga de Retardo — mina para recompensar movimento

Deixa minas na posição passada do jogador e pune a horda que o segue.

1. **Carga de Retardo** — deixa uma mina na posição anterior do jogador; após
   alguns segundos, ela explode em área e empurra inimigos.
2. **Estopim Curto** — reduz o atraso antes da explosão.
3. **Invólucro Expandido** — aumenta o raio da explosão.
4. **Compartimento Duplo** — a cada ativação, deixa duas cargas em posições
   recentes do jogador.
5. **Carga de Impacto** — aumenta dano e força de empurrão da explosão.
6. **Detonador de Proximidade** — depois de armadas, as cargas podem explodir
   antes do tempo ao detectar uma concentração de inimigos.

7. **Demolição em Cadeia** — evolução da Forja; explosões detonam cargas próximas
   em sequência e deixam fogo ou óleo no chão.

## Estado de implementação

Rebites de Pressão, Óleo Cru, Serras Orbitais, Estacas Hidráulicas e Tempestade
de Rebites já existem no protótipo, mas ainda não têm estas trilhas de sete
níveis. Prensa de Choque, Carga de Retardo e as demais evoluções estão aprovadas
como design e aguardam implementação.
