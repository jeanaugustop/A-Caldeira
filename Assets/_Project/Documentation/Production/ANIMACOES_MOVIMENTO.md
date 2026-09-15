# Animações de deslocamento

Implementadas em 14/09/2026 e integradas ao projeto principal em 15/09/2026.

## Resultado

- 4 quadros de movimento para cada uma das 8 direções de cada ator.
- 32 quadros por ator; 128 quadros de deslocamento no total, além das 32 poses paradas existentes.
- Engenheiro: corrida pesada com pernas, braços, avental e mochila reagindo à passada.
- Drone: trote mecânico alternando pares de pernas.
- Trator/tanque: avanço das esteiras, rodas e vibração do motor, sem fazê-lo saltitar.
- Prensa: passada hidráulica pesada alternando pernas e altura do corpo.
- A frequência dos quadros acompanha a distância percorrida. Ao parar, volta para a pose estática da última direção.
- Pausa congela a animação; reinício volta à pose frontal.

## Integração

Os PNGs ficam em Assets/_Project/Art/Dieselpunk/Atlases com os nomes engineer-run.png, drone-run.png, tractor-run.png e press-run.png. Cada arquivo usa 4 colunas por 8 linhas. Os assets DirectionalSprites guardam as 8 poses estáticas, os 32 quadros em movimento, escala e frequência.

O gerador forneceu o fundo quadriculado como pixels opacos. A tentativa de uma segunda extração pela ferramenta integrada encontrou o limite temporário do plano. Para não degradar ou redesenhar os quadros, AnimationSheetTransparency mantém os PNGs de origem intactos e remove apenas o padrão neutro conectado às bordas durante a importação do Unity. O instalador rejeita automaticamente uma prancha se o canto importado não tiver transparência.

O recortador seleciona o maior componente conectado dentro de cada célula, impedindo que partes de quadros vizinhos vazem para a animação.

## Validação

- 17 testes EditMode aprovados: oito direções, pose parada, pausa/reset e avanço dos quatro quadros.
- 2 testes PlayMode aprovados: partida, reinício, carga com 1.200 inimigos, 128 quadros não nulos, escala constante e captura visual.
- Prévias renderizadas em ArtPreview/Movimento-*.png.

Ainda é recomendável uma partida manual para ajustar o ritmo da corrida pelo gosto visual; a frequência fica em AnimationFramesPerUnit nos quatro assets *-directions.asset.

## Geração

Modo utilizado: ferramenta integrada de geração de imagens, usando as pranchas direcionais do próprio projeto como referências de identidade. Nenhuma CLI ou chave externa foi usada.

### engineer-run

Use case: identity-preserve.
Asset type: production Unity 2D movement sprite sheet.
Input image: exact identity, materials, colors, proportions, camera, lighting, and eight facing directions reference.
Primary request: make a genuinely animated four-frame locomotion cycle for each of the eight directions, while preserving the exact same actor design.
Composition: EXACT 4 columns x 8 rows grid, 32 isolated full-body sprites. Columns are consecutive animation frames 1,2,3,4. Rows, top to bottom, are facing screen DOWN/front, DOWN-LEFT/front diagonal, LEFT/profile, UP-LEFT/rear diagonal, UP/back, UP-RIGHT/rear diagonal, RIGHT/profile, DOWN-RIGHT/front diagonal.
Each cell must contain exactly one complete subject centered at identical scale and ground contact, with at least 8% transparent gutter and no overlap/cropping. Fixed elevated orthographic three-quarter survivor-game camera.
Background: actual transparent alpha=0 RGBA, not checkerboard, not dark, no floor, no cast shadow outside subject.
Constraints: four frames in each row must clearly differ in limb/tread position and together loop seamlessly; preserve identity and facing direction within the row; no idle duplicates; no added equipment; no text, labels, borders, separators, logos, watermark or extra objects.
Subject: engineer.
Motion: a strong running cycle: frame 1 left boot forward/right arm forward, frame 2 passing pose with torso lowest, frame 3 right boot forward/left arm forward, frame 4 airborne/passing pose with torso highest; backpack, apron and arms visibly react to the stride; energetic but heavy exosuit motion

### drone-run

Use case: identity-preserve.
Asset type: production Unity 2D movement sprite sheet.
Input image: exact identity, materials, colors, proportions, camera, lighting, and eight facing directions reference.
Primary request: make a genuinely animated four-frame locomotion cycle for each of the eight directions, while preserving the exact same actor design.
Composition: EXACT 4 columns x 8 rows grid, 32 isolated full-body sprites. Columns are consecutive animation frames 1,2,3,4. Rows, top to bottom, are facing screen DOWN/front, DOWN-LEFT/front diagonal, LEFT/profile, UP-LEFT/rear diagonal, UP/back, UP-RIGHT/rear diagonal, RIGHT/profile, DOWN-RIGHT/front diagonal.
Each cell must contain exactly one complete subject centered at identical scale and ground contact, with at least 8% transparent gutter and no overlap/cropping. Fixed elevated orthographic three-quarter survivor-game camera.
Background: actual transparent alpha=0 RGBA, not checkerboard, not dark, no floor, no cast shadow outside subject.
Constraints: four frames in each row must clearly differ in limb/tread position and together loop seamlessly; preserve identity and facing direction within the row; no idle duplicates; no added equipment; no text, labels, borders, separators, logos, watermark or extra objects.
Subject: four-legged welding drone.
Motion: a mechanical scuttle cycle: alternating diagonal pairs of legs step forward and back, body dips and rises, antenna and rear tank react subtly; feet positions must visibly differ in all four frames

### tractor-run

Use case: identity-preserve.
Asset type: production Unity 2D movement sprite sheet.
Input image: exact identity, materials, colors, proportions, camera, lighting, and eight facing directions reference.
Primary request: make a genuinely animated four-frame locomotion cycle for each of the eight directions, while preserving the exact same actor design.
Composition: EXACT 4 columns x 8 rows grid, 32 isolated full-body sprites. Columns are consecutive animation frames 1,2,3,4. Rows, top to bottom, are facing screen DOWN/front, DOWN-LEFT/front diagonal, LEFT/profile, UP-LEFT/rear diagonal, UP/back, UP-RIGHT/rear diagonal, RIGHT/profile, DOWN-RIGHT/front diagonal.
Each cell must contain exactly one complete subject centered at identical scale and ground contact, with at least 8% transparent gutter and no overlap/cropping. Fixed elevated orthographic three-quarter survivor-game camera.
Background: actual transparent alpha=0 RGBA, not checkerboard, not dark, no floor, no cast shadow outside subject.
Constraints: four frames in each row must clearly differ in limb/tread position and together loop seamlessly; preserve identity and facing direction within the row; no idle duplicates; no added equipment; no text, labels, borders, separators, logos, watermark or extra objects.
Subject: tracked scrap bulldozer tank.
Motion: a locomotion cycle: caterpillar tread links visibly advance one quarter step per frame, wheels rotate, chassis vibrates and exhaust stack rocks subtly; blade and whole vehicle remain structurally identical; no legs or deformation

### press-run

Use case: identity-preserve.
Asset type: production Unity 2D movement sprite sheet.
Input image: exact identity, materials, colors, proportions, camera, lighting, and eight facing directions reference.
Primary request: make a genuinely animated four-frame locomotion cycle for each of the eight directions, while preserving the exact same actor design.
Composition: EXACT 4 columns x 8 rows grid, 32 isolated full-body sprites. Columns are consecutive animation frames 1,2,3,4. Rows, top to bottom, are facing screen DOWN/front, DOWN-LEFT/front diagonal, LEFT/profile, UP-LEFT/rear diagonal, UP/back, UP-RIGHT/rear diagonal, RIGHT/profile, DOWN-RIGHT/front diagonal.
Each cell must contain exactly one complete subject centered at identical scale and ground contact, with at least 8% transparent gutter and no overlap/cropping. Fixed elevated orthographic three-quarter survivor-game camera.
Background: actual transparent alpha=0 RGBA, not checkerboard, not dark, no floor, no cast shadow outside subject.
Constraints: four frames in each row must clearly differ in limb/tread position and together loop seamlessly; preserve identity and facing direction within the row; no idle duplicates; no added equipment; no text, labels, borders, separators, logos, watermark or extra objects.
Subject: heavy bipedal press robot.
Motion: a heavy stomp cycle: left piston leg forward, compression/down pose, right piston leg forward, extension/up pose; hydraulic arms counter-swing, furnace torso visibly rises and falls, feet positions differ in every frame

### Prompt tentado para extração de fundo

Use case: background-extraction.
Asset type: Unity 2D animation sprite sheet.
Input image: edit target.
Primary request: remove every white and gray checkerboard square from the entire canvas and replace it with true transparent alpha=0.
Constraints: preserve all 32 character/robot/vehicle frames exactly in the same 4-column x 8-row positions, dimensions, colors, outlines and poses; keep every subject pixel; cleanly extract transparent spaces between arms, legs, tracks and machinery; output true RGBA transparency. Do not redraw, restyle, resize, crop, rearrange, add shadows, text or extra objects. The corners and all gutters must have alpha 0.
