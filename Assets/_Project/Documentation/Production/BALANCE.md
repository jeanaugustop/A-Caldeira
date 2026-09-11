# Notas de balanceamento

## Simulação inicial de XP — referência, não meta

Esta projeção usa a fórmula de XP atual e supõe que as três ondas do Pátio de
Triagem continuem com as mesmas frequências por 20 minutos. A fase atual dura
cinco minutos; portanto, os números de 10, 15 e 20 minutos são uma extrapolação
para orientar decisões, não o resultado de uma partida real.

Premissas:

- Drone: 2 XP, um spawn a cada 0,12 s desde o início.
- Trator: 5 XP, um spawn a cada 1,0 s a partir de 45 s.
- Prensa: 12 XP, um spawn a cada 1,4 s a partir de 90 s.
- Todos os drops têm chance de 100% no protótipo.
- A coluna 100% presume que todos os inimigos spawnados morrem e todo XP é
  coletado; 70% e 50% simulam eficiência parcial de combate/coleta.
- Arredondamentos de frame podem variar alguns pontos de XP.

| Minuto | XP spawnado estimado | Nível a 100% | Nível a 70% | Nível a 50% |
|---:|---:|---:|---:|---:|
| 5 | 8.075 | 22 | 20 | 17 |
| 10 | 17.143 | 30 | 26 | 23 |
| 15 | 26.211 | 35 | 30 | 27 |
| 20 | 35.279 | 39 | 34 | 30 |

## Como usar esta referência

Não decidir o nível máximo das armas apenas por esta tabela. A curva real depende
de dano, sobrevivência, coleta, escolhas de build, bosses e ondas futuras.

Antes de fechar o sistema de armas, registrar em playtests reais:

- nível do jogador nos minutos 5, 10, 15 e 20;
- quantidade de escolhas de upgrade feitas;
- armas desbloqueadas e upgrades escolhidos;
- mortes, XP deixado no chão e momentos de queda de ritmo.

Com esses dados, dividir o orçamento de escolhas entre atributos gerais,
desbloqueios, níveis de arma e evoluções.

## Meta inicial aprovada

A referência inicial para uma run de 20 minutos é chegar aproximadamente ao
**nível 45**: são 44 escolhas depois do nível inicial. Esta não é uma promessa de
que toda run chegue exatamente lá; serve como alvo de balanceamento antes dos
playtests da versão de 20 minutos.

Com a mesma projeção de XP, um fator quadrático de aproximadamente **0,5** na
fórmula de custo de nível aproxima esse alvo. Não alterar o código ainda: validar
primeiro o ritmo de escolhas e a dificuldade da run expandida.

## Raridade dos atributos — tabela inicial aprovada

Todo atributo básico usa o mesmo percentual conforme a raridade do card. O
percentual é aplicado ao efeito apropriado: dano, velocidade e raio de coleta
aumentam; o intervalo entre disparos diminui.

| Raridade | Bônus do card |
|---|---:|
| Comum | 4% |
| Incomum | 7% |
| Rara | 10% |
| Épica | 13% |
| Lendária | 16% |

Esta é a primeira tabela para playtest. Os valores crescem em três pontos
percentuais por raridade e não diminuem quando um mesmo atributo é escolhido
mais de uma vez.

### Regras das ofertas

- Um atributo básico não tem limite de cópias. Seu peso de aparição diminui, mas
  nunca chega a zero: começa em 100 e segue aproximadamente 67, 50, 40 e 29
  após 1, 2, 3 e 5 cópias escolhidas.
- Toda tela de escolha possui uma arma, um atributo geral e um acessório. Como
  existe apenas uma posição para atributo, o mesmo atributo não se repete na
  mesma tela.
- O bônus de cada card é somado ao valor-base do atributo, não ao valor já
  modificado. Três cards Comuns somam exatamente 12%.

As regras completas dos atributos, incluindo a tabela de Sorte, estão em
`ATRIBUTOS.md`.

Se não houver candidato elegível na posição de arma ou acessório, ela é
substituída por um atributo geral para a tela continuar com três opções.

### Cadência de Disparo

O atributo antes chamado de Válvula Rápida passa a comunicar **Cadência de
Disparo**. O percentual aumenta a cadência sobre o valor-base, em vez de reduzir
um intervalo até ele se tornar negativo. Assim, 100% de cadência dispara duas
vezes mais rápido e 400% dispara cinco vezes mais rápido.

Cadência afeta todas as armas automáticas, inclusive armas desbloqueadas,
evoluções e a recriação de armas orbitais. Não há teto de balanceamento definido
por enquanto; só será incluída uma proteção técnica alta caso um playtest revele
excesso de projéteis ou problema de desempenho.

## Slots de equipamento

Cada run terá **oito slots compartilhados** de equipamento. Armas e acessórios
ocupam o mesmo tipo de slot, permitindo builds como seis armas e dois acessórios,
quatro armas e quatro acessórios ou duas armas especializadas e seis acessórios.

Atributos gerais — Pressão Extra, Servo Rápido, Eletroímã, Cadência de Disparo e
Sorte — não ocupam slots. Ao subir de nível, as ofertas poderão misturar novos
equipamentos, melhorias de armas já equipadas, acessórios e atributos gerais. Ao
preencher os oito slots, novos equipamentos deixam de aparecer nas ofertas.

## Esquiva e rerolls

**Esquiva** é um atributo geral que nega completamente um dano quando ativada.
Usa a mesma tabela de bônus por raridade dos demais atributos e tem teto de 65%
de chance total, para uma build defensiva continuar exposta a risco.

Toda run começa com três rerolls das opções de aprimoramento. Rerolls não ocupam
slot; uma fonte adicional de +3 rerolls poderá ser criada futuramente como
recompensa ou item específico, se os playtests indicarem necessidade.

Cada reroll troca as três opções de uma vez — arma, atributo geral e acessório —
e preserva essa estrutura. Será acionado por botão visível ou pela tecla `R` no
PC.

## Modelo de níveis das armas

### Regra vigente

Cada arma tem sete níveis. Os níveis 1 a 6 avançam por cards de melhoria e o
jogador escolhe qual arma avançar. O nível 7 é uma evolução: na Forja, duas
armas no nível 6 são fundidas, consumindo as duas e criando uma arma evoluída.
Isso libera um slot de equipamento. As duplas específicas de cada fusão serão
decididas posteriormente. Esta regra substitui anotações anteriores que deixavam
o nível máximo de cada arma em aberto.

Cada arma terá uma sequência própria de níveis com efeitos predeterminados. Um
card de melhoria da arma avança para seu próximo nível conhecido — por exemplo,
o próximo nível de Rebites pode adicionar um projétil. Assim, o jogador escolhe
**qual arma** melhorar, mas não precisa escolher um atributo interno da arma a
cada nível.

O orçamento das 44 escolhas não terá cotas obrigatórias para armas, acessórios
e atributos gerais. A composição será determinada pela build escolhida pelo
jogador; os playtests vão indicar apenas se a frequência das ofertas permite
escolhas interessantes. O nível máximo e a sequência de cada arma serão
definidos individualmente antes da implementação.

## Primeira leva de acessórios

Os acessórios são únicos por run, não recebem raridade e não substituem os
atributos gerais. Seus valores exatos serão calibrados em playtest.

| Acessório | Efeito aprovado |
|---|---|
| Anel de Contingência | A cada 40 s, bloqueia o próximo dano recebido. |
| Fusível Sacrificial | Ao receber dano letal, retorna com 1 de vida, fica invulnerável por 5 s e é consumido. |
| Válvula de Pânico | Ao receber dano, concede grande velocidade por 3 s, com recarga. |
| Placa de Amortecimento | Reduz uma porcentagem fixa do dano recebido. |
| Bobina de Recolhimento | A cada 40 s, puxa o XP visível em direção ao jogador. |
| Sirene de Contenção | Periodicamente empurra e desacelera inimigos próximos. |
| Cabo de Aterramento | Ao bloquear um dano, emite um pulso que empurra inimigos próximos. |

## Inventário atual de progressão

| Grupo | Conteúdo | Escolhas possíveis hoje |
|---|---|---:|
| Atributos gerais | Pressão Extra (5), Servo Rápido (4), Eletroímã (4), Válvula Rápida (5) | 18 |
| Desbloqueios de arma | Óleo Cru, Serras Orbitais, Estacas Hidráulicas | 3 |
| Evolução | Tempestade de Rebites | 1 |
| **Total de escolhas atuais** | 8 cards/itens distintos | **22** |

Armas em código: Rebites de Pressão (inicial), Óleo Cru, Serras Orbitais e
Estacas Hidráulicas; Tempestade de Rebites é evolução da arma inicial. Prensa de
Choque e Carga de Retardo estão aprovadas no backlog, mas ainda não existem no
jogo.

Para a meta de 44 escolhas, faltam 22 escolhas além do conteúdo limitado atual.
O sistema planejado de atributos básicos repetíveis cobre essa diferença, mas
precisará de variedade suficiente para não repetir sempre os mesmos quatro cards.
