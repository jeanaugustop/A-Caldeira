# Jogador e sobrevivência

## Base atual do protótipo

- Vida máxima inicial: 100.
- Após receber dano, o jogador tem uma breve invulnerabilidade para não sofrer
  vários acertos no mesmo instante.
- Atributos gerais atuais: Pressão Extra, Servo Rápido, Eletroímã, Cadência de
  Disparo, Esquiva e Sorte. Consulte `ATRIBUTOS.md`.

## Proposta de progressão automática de vida

A vida máxima aumenta automaticamente a cada dez níveis do jogador. Isso não
ocupa slot e não concorre com armas, acessórios ou atributos gerais.

| Nível alcançado | Vida máxima proposta |
|---:|---:|
| 1 | 100 |
| 10 | 120 |
| 20 | 140 |
| 30 | 160 |
| 40 | 180 |

Cada marco adiciona 20 de vida máxima. Com a meta de nível 45 em uma run de 20
minutos, o jogador pode chegar a 180 de vida máxima antes do chefe final.

## Proposta de regeneração

- Regeneração base: 1 ponto de vida por segundo.
- A intenção é compensar erros pequenos ao longo da run, sem transformar dano
  pesado em algo irrelevante.
- Para preservar tensão, avaliar no primeiro playtest se a regeneração deve
  começar apenas após alguns segundos sem receber dano. Essa condição ainda não
  está decidida.

## Proteções e recuperação

- Esquiva pode negar dano, com teto de 65% de chance total.
- Anel de Contingência bloqueia um dano periodicamente.
- Fusível Sacrificial evita uma morte letal uma vez, retorna o jogador com 1 de
  vida e concede cinco segundos de invulnerabilidade.
- Placa de Amortecimento reduz dano recebido.

Esses sistemas devem funcionar junto com a regeneração, sem depender de uma cura
ativa ou de drops de cura nesta primeira versão.
