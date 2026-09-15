# Versão com artes dieselpunk

Passagem visual iniciada em 14/09/2026 e integrada ao projeto principal em 15/09/2026.

## Direção visual

Dieselpunk industrial: aço escuro, tinta gasta, ferrugem, pistões, motores, cobre e luz âmbar. O engenheiro usa verde-petróleo; inimigos têm silhuetas e cores próprias. Sprites raster com transparência, perspectiva elevada de três quartos e acabamento pintado.

As imagens originais estão em `Assets/_Project/Art/Dieselpunk/Atlases`. O Unity recorta as pranchas sem alterar os PNGs. Os prompts e a origem estão em `ART_PROMPTS.md`.

## Conteúdo desta passagem

- Personagem em exotraje e os três inimigos existentes.
- Ícones para as seis armas aprovadas, incluindo Prensa de Choque e Carga de Retardo; estes dois ícones não significam que as armas já foram implementadas.
- Sete acessórios e sucata coletável.
- Caldeira, barris, gerador, caixas, fornalha e barreira industrial.
- Rebite, bolota de óleo, poça, estaca, círculo de aviso e faíscas.
- Piso modular, fundo de menu, cards com ícones e oito slots de equipamento.
- Personagem e inimigos com oito direções, proporções próprias e quatro quadros de deslocamento em cada direção (ver `SPRITES_DIRECIONAIS.md` e `ANIMACOES_MOVIMENTO.md`).

## Um mapa maior

O foco jogável é o Pátio de Triagem, agora com **180 x 120 unidades**, antes 60 x 40: três vezes cada dimensão, nove vezes a área. A duração continua sendo cinco minutos.

Setores do mesmo mapa: Praça de Triagem, Pátio das Caldeiras, Depósito de Combustível, Casa das Máquinas e Docas de Sucata. Há avenidas largas e ilhas de máquinas com bloqueio de movimentação. Os inimigos continuam nascendo perto do jogador, preservando o ritmo de combate ao explorar.

A segunda fase e o teste técnico foram retirados das opções visíveis do menu, mas seus arquivos continuam disponíveis para desenvolvimento. A Forja e o chefe continuam pendentes.

## Como abrir

Adicionar a pasta principal `A Caldeira_ Sobrevivência de Ferro` no Unity Hub. Usar Unity 2022.3.62f3, a versão registrada neste projeto. Abrir `Assets/_Project/Generated/Scenes/00_Bootstrap.unity`, dar Play e selecionar Pátio de Triagem.

O instalador `Tools > A Caldeira > Aplicar artes dieselpunk` reaplica os recortes e a composição visual. Ele reconstitui o cenário do Pátio; salvar uma cópia da cena antes de personalizá-la e reaplicar o instalador.

Os saves usam o produto principal `A Caldeira - Sobrevivencia de Ferro`. As pastas de cache e builds continuam locais e não devem ser versionadas.

## Próxima passagem artística

- Validar leitura das silhuetas, poças e avisos com horda cheia.
- Refinar os ciclos de deslocamento após playtest e desenhar animações específicas de ataque/dano.
- Refinar sombras, partículas e efeitos sonoros.
- Rebalancear distâncias e obstáculos após playtests no Pátio maior.

## Verificação

- Compilação e aplicação das cenas pelo Unity 2022.3.62f3.
- Teste PlayMode de partida no mapa maior, mira na extremidade, cards com ícones, rerrolagem, pausa e reinício.
- Teste PlayMode de carga com 1.200 inimigos, reinício e devolução dos objetos ao pool.
- Teste PlayMode do Óleo Cru, confirmando duas quedas separadas, dano e lentidão.
- Prévias renderizadas pelo jogo em `ArtPreview`: menu, Pátio, extremidade do mapa e aprimoramentos.

Esses testes não substituem uma partida manual completa para avaliar diversão, desempenho e leitura visual com todos os equipamentos.
