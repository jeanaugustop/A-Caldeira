# Curva de combate

## Referência atual do protótipo

| Entidade | Vida | Dano de contato |
|---|---:|---:|
| Jogador | 100 | — |
| Drone de Solda | 18 | 6 |
| Trator Sucatador | 65 | 12 |
| Prensa Autônoma | 180 | 25 |

O jogador tem uma breve invulnerabilidade após receber dano. Esta base será
mantida como referência para o primeiro balanceamento da run expandida.

## Estado atual do playtest

O **Óleo Cru** voltou às escolhas em uma forma nova: cospe bolotas que caem a uma
distância fixa e deixam poças de dano baixo. A base é uma ativação a cada 3 s;
o comportamento está implementado, mas seus números ainda aguardam playtest.

A fase atual é o Pátio de Triagem de **5 minutos**, com área de 180 × 120. Os
três inimigos ainda usam os valores-base desta tabela e entram por ondas simples
ao longo da partida. Não há escalonamento automático por vida/dano, elite, onda
especial ou chefe no runtime atual.

## Curva inicial para uma run de 20 minutos

| Momento | Vida inimiga | Quantidade de inimigos | Objetivo da fase |
|---|---:|---:|---|
| 0–5 min | x1 | x1 | Aprender o loop e obter os primeiros equipamentos. |
| 5–10 min | x1,25 | x1,3 | Primeira onda especial; a build começa a importar. |
| 10–15 min | x1,55 | x1,6 | A horda exige área, controle ou movimento. |
| 15–20 min | x1,95 | x2 | Pressão máxima antes do chefe. |
| 20 min | Chefe próprio | — | Marco final da run. |

## Princípios aprovados

- A dificuldade deve crescer mais por quantidade, velocidade, composição e ondas
  especiais do que por dano bruto.
- O dano dos inimigos pode aumentar pouco nos blocos finais, mas não deve matar o
  jogador em dois ou três toques sem espaço de reação.
- Esquiva, Anel de Contingência, Placa de Amortecimento e movimentação precisam
  continuar tendo valor até o final da run.
- Os números de dano, recarga, área e duração das armas serão calibrados contra
  esta curva em playtests, começando pelo novo Óleo Cru.
