# Roadmap de produção — A Caldeira

## Objetivo atual

Transformar o protótipo tecnicamente validado em uma vertical slice: uma versão
curta, estável e apresentável que mostra o combate, a progressão e a identidade
industrial do jogo.

## Ondas

| Onda | Foco | Critério de saída |
|---|---|---|
| 0 — Base técnica | Validação, build e versionamento | Concluída: testes, Bootstrap, carga e GitHub aprovados |
| 1 — Polimento do Pátio | Sensação de jogo, combate e clareza dos primeiros minutos | Um jogador novo entende, sobrevive e quer repetir a partida |
| 2 — Conteúdo | Direção visual/sonora, segunda fase e variedade | Duas fases distintas com conteúdo final suficiente para demonstração |
| 3 — Progressão e UX | Oficina, tutorial, acessibilidade e menus | Loop completo compreensível sem explicação externa |
| 4 — Qualidade | Correções, compatibilidade e desempenho após o conteúdo | Build candidata a demo, sem falhas bloqueadoras |
| 5 — Publicação | Loja, trailer e distribuição | Demo ou lançamento definido e publicado |

## Regra de priorização

1. Só entra conteúdo novo quando a experiência dos primeiros cinco minutos está
   clara e agradável.
2. Uma tarefa precisa ter um resultado observável e um critério de aceite.
3. Mudanças de desempenho devem repetir o teste de carga antes de serem dadas
   como prontas.

## Próximo marco

Concluir a Onda 1 com os efeitos básicos das armas/acessórios claros e três
playtests externos registrados. O Óleo Cru em poças foi validado em 15/09/2026.
A arte dieselpunk, cards, slots e animações já fazem parte desta onda; o que
falta agora é confirmar a diversão e o balanceamento em partida real.

## Direção de design aprovada

- A versão expandida terá runs de aproximadamente 20 minutos.
- Ondas especiais marcarão a escalada de dificuldade durante a run.
- Um chefão aparecerá no minuto 20 e será o marco final da rodada.
- Evoluções de arma serão obtidas em uma forja presente em cada mapa, em vez de
  dependerem de missões de mira incompatíveis com o combate automático.
- Armas, cura e variedade adicional serão incrementadas depois que esse loop
  estiver validado; o protótipo atual não precisa mudar imediatamente.
- O jogo terá dois modos separados: **Sobrevivência**, atualmente em produção,
  e um futuro **Modo História** baseado em missões e setores da empresa.
- A campanha reutilizará combate, armas, inimigos, Forja e progressão já
  validados no modo Sobrevivência; sua produção só começa depois da vertical
  slice atual estar estável.
- A premissa narrativa aprovada está registrada em `LORE.md`.
