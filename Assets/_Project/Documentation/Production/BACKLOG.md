# Backlog de produção

Atualizado em 15/09/2026 após revisão de código, cenas, arte e documentação.
Este arquivo é a fonte única de pendências. As regras e trilhas aprovadas vivem
nos documentos específicos de armas, acessórios e atributos.

## Em andamento

- [x] Validar o novo **Óleo Cru** em playtest.
  - A arma lança bolotas a cada 3 s, que caem a uma distância fixa e viram poças
    de dano baixo; nível 2 cria duas quedas, nível 3 desacelera e nível 6 cria
    poças reforçadas periodicamente.
  - Resultado em 15/09/2026: **aprovado**. Não foram solicitados novos
    ajustes de balanceamento neste teste.
- [x] Registrar esse teste em `PLAYTESTS.md`.
  - O commit/build não foi anotado durante a sessão; registrar essa informação
    nos próximos playtests quando estiver disponível.

## Prioridade 0 — confiabilidade e validação

- [ ] Descobrir por que a referência serializada de `WaveSpawner` pode ficar nula
  durante uma partida e remover a busca de recuperação dentro do `Update`.
  - Aceite: nenhuma busca de cena por quadro; troca de cena, reinício e Play Mode
    não produzem `NullReferenceException`.
- [ ] Rodar e registrar os testes EditMode e PlayMode no Unity 2022.3.62f3.
  - O repositório contém testes de pools/grade, ciclo de partida, arte direcional
    e Óleo Cru; o resultado mais recente deve ser registrado, não presumido.
- [ ] Gerar uma Development Build Windows e fazer uma partida manual completa.
  - Aceite: menu, Pátio, pausa, rerroll, fim de partida e reinício funcionam na
    build, sem depender do Editor.

## Prioridade 1 — combate e progressão já visíveis ao jogador

- [ ] Implementar os efeitos únicos ainda faltantes das armas atuais.
  - [x] **Rebites:** área/tamanho real e Pistão de Impacto com dano, perfuração
    e empurrão próprios. Aguardar playtest de balanceamento.
  - [x] **Serras:** progressão de uma a quatro serras, raio, rotação e tamanho
    próprios. Aprovadas em playtest em 16/09/2026.
  - [x] **Estacas:** tiro paralelo, perfuração, alcance, empurrão e trilho de
    pressão reais. Aprovadas em playtest em 16/09/2026.
  - **Óleo:** fechar o balanceamento após o playtest atual.
- [ ] Completar os efeitos de níveis 1–6 dos acessórios.
  - A seleção, slots, recargas e efeitos básicos existem, mas vários efeitos de
    card ainda não estão em runtime: descarga do Cabo em esquiva, pulso danoso,
    melhorias reativas da Placa e efeitos avançados do Fusível, Bobina e Válvula
    de Pânico.
  - [x] **Sirene de Contenção:** níveis 1–6 implementados; aguarda playtest.
  - [x] **Cabo de Aterramento:** níveis 1–6 implementados e aprovados em
    playtest em 16/09/2026.
  - [x] **Anel de Contingência:** níveis 1–6 implementados e aprovados em
    playtest em 16/09/2026.
  - [x] **Fusível Sacrificial:** níveis 1–6 implementados; aguarda playtest.
- [ ] Corrigir a regra de ofertas extras de atributo quando arma e acessório não
  têm candidato elegível.
  - Aceite: continuam três cards, mas um atributo não aparece duplicado na mesma
    tela por causa do fallback.

## Prioridade 2 — variedade de conteúdo

- [ ] Adicionar **Prensa de Choque**.
  - Ataque automático na maior concentração, dano em área e empurrão.
  - Os níveis e a evolução **Linha de Montagem** estão em `ARMAS.md`.
- [ ] Adicionar **Carga de Retardo**.
  - Mina na posição anterior do jogador, explosão em área e empurrão.
  - Os níveis e a evolução **Demolição em Cadeia** estão em `ARMAS.md`.
- [ ] Após as seis armas base estarem jogáveis, revisar números e papéis de cada
  arma antes de ativar evoluções.

## Prioridade 3 — loop da partida

- [ ] Criar eventos para a run atual de 5 minutos: picos de horda, inimigos
  resistentes ou ondas especiais que comuniquem a progressão.
- [ ] Definir e implementar a evolução da dificuldade sem depender apenas de
  vida/dano bruto.
- [ ] Só depois, expandir para aproximadamente 20 minutos, com marcos em 5, 10
  e 15 minutos e um chefe no minuto 20.

## Prioridade 4 — Forja e evoluções

- [ ] Criar a Forja em um ponto próprio de cada mapa.
- [ ] Pausar a simulação ao ativá-la e abrir uma tela de escolha, não um loading.
- [ ] Fundir duas armas nível 6, consumir ambas, liberar um slot e criar a arma
  evolução nível 7.
- [ ] Definir receitas, custos, limites por run e as evoluções de acessórios.

## Playtests e qualidade

- [ ] Convidar três pessoas que não participaram do desenvolvimento para jogar.
- [ ] Observar o primeiro minuto sem explicar controles e registrar confusões,
  mortes, escolhas e momentos divertidos em `PLAYTESTS.md`.
- [ ] Ajustar movimento, leitura de colisão, VFX/SFX de tiro/acerto/dano/coleta e
  o ritmo de XP conforme os dados desses testes.
- [ ] Medir o teste de carga e uma partida real em build, incluindo GC, p95 de
  frame time e esgotamento de pools.

## Fora do escopo agora

- Publicação em loja, Steam, monetização, multiplayer e backend.
- Terceira fase, otimização com Jobs/Burst sem evidência do profiler e chefe antes
  de o loop atual de 5 minutos estar validado.
- O **Modo História** completo, suas missões, diálogos e campanha. A lore e a
  separação entre os modos já estão aprovadas em `LORE.md`, mas a produção da
  campanha começa somente após a vertical slice do modo Sobrevivência.
