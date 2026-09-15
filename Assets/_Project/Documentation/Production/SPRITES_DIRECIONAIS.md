# Sprites direcionais e escala

Integração visual do projeto principal.

## Comportamento

- Engenheiro, drone de solda, trator sucatador e prensa autônoma têm poses para oito direções: frente, costas, laterais e diagonais.
- A direção acompanha o deslocamento real. Ao parar, conserva a última pose; pausa e reinício são tratados separadamente.
- Sem girar a imagem plana ao andar para cima. Perfis e costas usam sprites próprios; a diagonal frontal esquerda do engenheiro e do drone usa espelhamento para corrigir a orientação da prancha.
- Em movimento, cada direção usa um ciclo próprio de quatro quadros. O engenheiro corre, drone e prensa articulam as pernas e o trator anima a locomoção das esteiras sem saltitar. Ver `ANIMACOES_MOVIMENTO.md`.
- Atualização dos inimigos dentro da simulação existente, sem um Update extra por inimigo.

## Escala de referência

Medida visual do maior eixo do sprite, em unidades do jogo; não são metros reais.

| Ator | Tamanho visual | Raio de contato |
| --- | ---: | ---: |
| Drone | 1,0 | 0,30 |
| Engenheiro | 1,8 | Mantido |
| Prensa autônoma | 2,8 | 0,75 |
| Trator / tanque | 4,2 | 1,05 |

Contato, obstáculos, nascimento dos inimigos, colisão de projéteis e contato com poças consideram o raio do inimigo. Vida, dano, velocidade, XP e cadência não foram alterados.

## Arquivos e edição

Pranchas: Assets/_Project/Art/Dieselpunk/Atlases/*-directions.png.
Configurações: Assets/_Project/Art/Dieselpunk/*-directions.asset (Frames, Mirror, WorldSize, BobAmplitude).

Menu de reaplicação: Tools > A Caldeira > Aplicar sprites direcionais. Esse comando atualiza a cena 00_Bootstrap e os dados de inimigos; não recria o mapa. Faça uma cópia da cena antes de reaplicar sobre alterações manuais.

Para jogar, abrir 00_Bootstrap com Unity 2022.3.62f3. O instalador visual geral também preserva a integração direcional quando as quatro pranchas existem.

## Origem e prompts

Validação atual: 17 testes EditMode e 3 PlayMode passaram. Incluem as oito direções, quatro quadros por direção, manutenção da pose parado/pausado, reset, proporções, reinício, Óleo Cru e carga com 1.200 inimigos. Prévias estão em `ArtPreview/Movimento-*.png`. Ainda é necessário playtest manual para avaliar o ritmo visual e o equilíbrio do contato com o tanque maior.

Imagens geradas com a ferramenta integrada de geração de imagens, usando actors.png como referência de identidade e estilo. Nenhuma API externa por CLI foi usada. Recorte não destrutivo pelo importador do Unity, preservando o alpha dos PNGs.

### engineer-directions

Use case: stylized-concept. Asset type: production Unity 2D eight-direction sprite atlas. Reference image: identity and material/style reference ONLY, not layout. Create exactly EIGHT complete views of the SAME subject, in a precise evenly spaced 4-column x 2-row grid on a genuinely transparent RGBA background. No text, borders, captions, ground, shadows outside subject, or extra objects. Camera is fixed elevated orthographic three-quarter top-down as in a survivor game, looking down 35 degrees. Subject rotates around vertical axis, camera and lighting remain fixed, not flat rotated copies. Directions row-major: TOP ROW facing screen DOWN (front), DOWN-LEFT (front three-quarter), LEFT (side), UP-LEFT (rear three-quarter). BOTTOM ROW facing UP (back), UP-RIGHT (rear three-quarter), RIGHT (side), DOWN-RIGHT (front three-quarter). All views same physical scale, each centered in its equal-sized cell with generous transparent padding at least 12%, no overlap or cropped limbs. Painted industrial dieselpunk, worn metal, copper, warm highlights, crisp readable silhouettes. Keep exactly the same construction and colors in all eight views.
Use ONLY the human engineer in the TOP LEFT of the reference. Full-body teal armored engineer with amber horizontal helmet visor, brown leather apron, heavy boots, cylindrical backpack diesel tank and pipes. Eight standing ready directional poses. No weapon added. Rear views clearly show backpack and back of helmet, no visor on back. Keep human proportions and same height across views.

### drone-directions

Use case: stylized-concept. Asset type: production Unity 2D eight-direction sprite atlas. Reference image: identity and material/style reference ONLY, not layout. Create exactly EIGHT complete views of the SAME subject, in a precise evenly spaced 4-column x 2-row grid on a genuinely transparent RGBA background. No text, borders, captions, ground, shadows outside subject, or extra objects. Camera is fixed elevated orthographic three-quarter top-down as in a survivor game, looking down 35 degrees. Subject rotates around vertical axis, camera and lighting remain fixed, not flat rotated copies. Directions row-major: TOP ROW facing screen DOWN (front), DOWN-LEFT (front three-quarter), LEFT (side), UP-LEFT (rear three-quarter). BOTTOM ROW facing UP (back), UP-RIGHT (rear three-quarter), RIGHT (side), DOWN-RIGHT (front three-quarter). All views same physical scale, each centered in its equal-sized cell with generous transparent padding at least 12%, no overlap or cropped limbs. Painted industrial dieselpunk, worn metal, copper, warm highlights, crisp readable silhouettes. Keep exactly the same construction and colors in all eight views.
Use ONLY the orange spider welding drone in the TOP RIGHT of the reference. Compact round orange steel torso, glowing red FRONT eye, four short articulated mechanical legs, rear tank and antenna. Eight locomotion-ready directional poses of this exact same small robot. Back views show exhaust and rear plating instead of a front eye. All feet fully visible.

### tractor-directions

Use case: stylized-concept. Asset type: production Unity 2D eight-direction sprite atlas. Reference image: identity and material/style reference ONLY, not layout. Create exactly EIGHT complete views of the SAME subject, in a precise evenly spaced 4-column x 2-row grid on a genuinely transparent RGBA background. No text, borders, captions, ground, shadows outside subject, or extra objects. Camera is fixed elevated orthographic three-quarter top-down as in a survivor game, looking down 35 degrees. Subject rotates around vertical axis, camera and lighting remain fixed, not flat rotated copies. Directions row-major: TOP ROW facing screen DOWN (front), DOWN-LEFT (front three-quarter), LEFT (side), UP-LEFT (rear three-quarter). BOTTOM ROW facing UP (back), UP-RIGHT (rear three-quarter), RIGHT (side), DOWN-RIGHT (front three-quarter). All views same physical scale, each centered in its equal-sized cell with generous transparent padding at least 12%, no overlap or cropped limbs. Painted industrial dieselpunk, worn metal, copper, warm highlights, crisp readable silhouettes. Keep exactly the same construction and colors in all eight views.
Use ONLY the yellow tracked tractor/tank in the BOTTOM LEFT of the reference. Heavy yellow diesel scrap bulldozer with TWO long caterpillar tracks, broad FRONT dozer scoop blade, boxy engine body, exhaust stack. Eight directional views of this exact same vehicle. Blade must rotate with body: down on front view, left on left view, at top BEHIND body on back view, right on right view. Back views prominently show engine rear, never a front radiator facing viewer. Large grounded heavy proportions.

### press-directions

Use case: stylized-concept. Asset type: production Unity 2D eight-direction sprite atlas. Reference image: identity and material/style reference ONLY, not layout. Create exactly EIGHT complete views of the SAME subject, in a precise evenly spaced 4-column x 2-row grid on a genuinely transparent RGBA background. No text, borders, captions, ground, shadows outside subject, or extra objects. Camera is fixed elevated orthographic three-quarter top-down as in a survivor game, looking down 35 degrees. Subject rotates around vertical axis, camera and lighting remain fixed, not flat rotated copies. Directions row-major: TOP ROW facing screen DOWN (front), DOWN-LEFT (front three-quarter), LEFT (side), UP-LEFT (rear three-quarter). BOTTOM ROW facing UP (back), UP-RIGHT (rear three-quarter), RIGHT (side), DOWN-RIGHT (front three-quarter). All views same physical scale, each centered in its equal-sized cell with generous transparent padding at least 12%, no overlap or cropped limbs. Painted industrial dieselpunk, worn metal, copper, warm highlights, crisp readable silhouettes. Keep exactly the same construction and colors in all eight views.
Use ONLY the dark bipedal press robot in the BOTTOM RIGHT of the reference. Heavy broad steel machine, two sturdy piston legs with rectangular feet, squared press chest with orange glowing FRONT vents and furnace jaw, hydraulic arms, rear boiler and pipes. Eight locomotion-ready directional poses. Back views show solid back armor and boiler, not face/vents. Preserve its bulky machine proportions.

### Correção de transparência

Use case: background-extraction. Edit target: this exact eight-view sprite atlas. Remove ALL the visible white/gray checkerboard from the background and from spaces between limbs. Replace it with ACTUAL fully transparent alpha=0 pixels, NOT a painted checker pattern, not a solid colored backdrop. Preserve all eight subjects exactly: same position, scale, colors, edges, direction, details. Do not redraw, reframe, crop, rearrange, add shadows or words. Output transparent RGBA PNG with eight isolated sprites in the same 4x2 grid. Critical: transparency must be an actual alpha channel, including the entire canvas corners and gutters.

undefined
