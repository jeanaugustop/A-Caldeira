# Jogador e sobrevivência

## Base atual do protótipo

- Vida máxima inicial: 100.
- Após receber dano, o jogador tem uma breve invulnerabilidade para não sofrer
  vários acertos no mesmo instante.
- Atributos gerais atuais: Pressão Extra, Servo Rápido, Eletroímã, Cadência de
  Disparo, Esquiva e Sorte. Consulte `ATRIBUTOS.md`.

## Progressão automática de vida implementada

A vida máxima aumenta automaticamente a cada dez níveis do jogador. Isso não
ocupa slot e não concorre com armas, acessórios ou atributos gerais.

| Nível alcançado | Vida máxima |
|---:|---:|
| 1 | 100 |
| 10 | 120 |
| 20 | 140 |
| 30 | 160 |
| 40 | 180 |

Cada marco adiciona 20 de vida máxima. Com a meta de nível 45 em uma run de 20
minutos, o jogador pode chegar a 180 de vida máxima antes do chefe final.

## Regeneração implementada

- Regeneração base: 1 ponto de vida por segundo, continuamente durante a run.
- Ela compensa erros pequenos, mas não interrompe o dano de contato nem substitui
  Esquiva, Anel, Fusível ou Placa.
- O playtest ainda precisa decidir se a regeneração contínua está forte demais ou
  se deverá começar apenas alguns segundos após o último dano.

## Proteções e recuperação

- Esquiva pode negar dano, com teto de 65% de chance total.
- Anel de Contingência bloqueia um dano periodicamente.
- Fusível Sacrificial evita uma morte letal uma vez, retorna o jogador com 1 de
  vida e concede cinco segundos de invulnerabilidade.
- Placa de Amortecimento reduz dano recebido.

Esses sistemas devem funcionar junto com a regeneração, sem depender de uma cura
ativa ou de drops de cura nesta primeira versão.
