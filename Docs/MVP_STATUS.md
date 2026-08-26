# Dev Carrer Idle Game — status da versão 0.2.0

## Estado atual

O projeto é um idle híbrido para PC, controlado com mouse. Não existe progresso
offline: fechar o jogo congela a produção e carregar restaura o tempo salvo.
O ritmo da carreira depende da participação do jogador, sem espera obrigatória.

## Missões e progressão

- 18 missões principais em checklist, três por cargo antes de Dono.
- Nenhuma missão tem prazo, falha ou escolha errada.
- Estagiário: tutorial das cinco ações e compra do Codex, sem esperar ciclos.
- Dev Júnior: projetos reais e Codex Multiagentes.
- Dev Pleno: primeira contratação e trabalho em equipe.
- Dev Sênior: testes inteligentes, arquitetura e mentoria.
- Tech Lead: pipeline, crescimento do time e incidentes sem culpados.
- CTO: plataforma, estratégia e preparação da liderança.
- Dono: sequência final da ByteForge.

Cada missão abre com briefing, história leve de escritório, checklist e
recompensa. O próximo móvel correto recebe losango dourado, brilho e texto. A
atividade usa uma ação por vez, seguida apenas de 1–2 segundos de animação.
Produção automática continua rendendo bônus, mas nunca aparece como objetivo.

## Economia idle

A produção acontece em ciclos de 15 segundos. O jogador começa com R$ 120, o
valor exato do Codex Assistente. Multiagentes custam R$ 250 e só liberam em
Júnior. Contratações custam `R$ 300 × número do próximo colega` e só liberam em
Dev Pleno. Os custos e recompensas foram simulados na carreira inteira; nenhum
upgrade, contratação ou missão depende de esperar dinheiro passivo.

## Interface, áudio e acessibilidade

- Menu principal assimétrico e HUD reconstruído para 1280 × 720.
- Pause em `Esc`, com produção idle mantida durante a pausa.
- Salvar, voltar ao menu e rever tutorial pelo menu de pausa.
- Música, ambiência de escritório e sons específicos por atividade.
- Volumes separados para música, efeitos e ambiente.
- Tela cheia, resolução e qualidade gráfica.
- Texto ampliado, alto contraste e movimento reduzido.

## Save e migração

O formato atual é a versão 7. Há autosave depois de atividade, compra, missão,
promoção e decisão, além de salvar/carregar manualmente. O save registra missão,
checklist, tempo aberto, ciclo automático, equipe, automação, atributos e
decisões pendentes. Saves anteriores são migrados sem conceder progresso offline
e sem preservar contratações ilegais de Estagiário/Júnior.

## Cenas

| Faixa | Cena |
|---|---|
| Menu | `MainMenu` |
| Estagiário | `OfficeInternMVP` |
| Júnior e Pleno | `OfficeDeveloperMVP` |
| Sênior e Tech Lead | `OfficeLeadershipMVP` |
| CTO | `OfficeCtoMVP` |
| Dono | `OwnerFinale` |

## Testes automatizados

- 17/17 Edit Mode: checklists, economia sem espera, Codex, multiagentes,
  contratação, carreira completa, ausência de falha, save e migração.
- 11/11 Play Mode: movimento, clique nas estações, HUD, menu, tutorial,
  briefings/decisões, promoção, pausa com idle e sequência final.

## Antes do executável

1. Playtest humano completo de leitura, ritmo, som e conforto visual.
2. Ajustes finais de texto e volumes encontrados no playtest.
3. Só então gerar e validar o executável. Não há sistema de conquistas.
