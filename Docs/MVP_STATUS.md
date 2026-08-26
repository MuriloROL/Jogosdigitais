# Dev Carrer Idle Game — status da versão 0.2.0

## Estado atual

O projeto é um idle híbrido para PC, controlado com mouse. A carreira inteira
acontece no Dia 1: Alex entra às 09:00 como Estagiário e deve chegar ao cargo de
Dono perto das 17:00, após aproximadamente sete a oito horas com o jogo aberto.

Não existe progresso offline. Fechar o jogo congela a produção e carregar um
save restaura exatamente o tempo aberto registrado.

## Economia idle

A produção acontece em ciclos de 15 segundos. Alex produz um ponto por ciclo e
cada colega aumenta a base da equipe. Upgrades de pipeline multiplicam a
produção total em 25% por nível.

| Recurso | Regra |
|---|---|
| Contratação | Custa `R$ 180 × número do próximo colega`. |
| Capacidade | Começa em uma vaga e cresce em uma vaga por cargo, até seis. |
| Automação | Custa `R$ 450 × próximo nível`, com cinco níveis. |
| Dinheiro passivo | Cada ciclo paga pelo menos R$ 1, crescendo com a produção. |
| Meta da sprint | Pode ser resgatada uma vez por cargo e paga dinheiro e 25 XP. |
| Ações manuais | Aceleram entrega e atributos sem adiantar o relógio real. |

Com contratações feitas quando ficam disponíveis, a simulação automatizada
chega a Dono em cerca de 7h18. Ações manuais e automação permitem terminar um
pouco antes, recompensando participação sem tornar cliques obrigatórios.

## Progressão

| Cargo | XP acumulado para promoção | Salário mensal |
|---|---:|---:|
| Estagiário | 350 | R$ 1.800 |
| Dev Júnior | 900 | R$ 3.500 |
| Dev Pleno | 1.700 | R$ 6.500 |
| Dev Sênior | 2.800 | R$ 10.000 |
| Tech Lead | 4.200 | R$ 14.000 |
| CTO | 7.500 | R$ 22.000 |
| Dono | Final | R$ 50.000 |

Cada promoção paga uma parcela do novo salário, abre capacidade de equipe,
troca o ambiente quando necessário e apresenta uma conversa com escolha. A
última promoção carrega diretamente a sequência final.

## Variedade ativa

- Cinco estações: programação, reunião, estudo, café e pausa.
- Sete tarefas rotativas com objetivos e recompensas próprias.
- Cinco conversas de promoção com duas decisões cada.
- Escolhas alteram caixa, XP, conhecimento, reputação e estresse.
- Estações possuem feedback visual, som e recarga para impedir repetição rápida.
- O painel idle mostra produção, próximo ciclo, equipe, pipeline e custos.
- O ciclo de produção produz um pulso visual discreto que respeita movimento
  reduzido.

## Polimento e acessibilidade

- Música ambiente persistente e efeitos procedurais para ações, botões,
  compras, escolhas e promoções.
- Configurações persistentes para música e efeitos.
- Modo de movimento reduzido, incluindo conclusão abreviada da cena final.
- Texto ampliado por ajuste da escala de referência da interface.
- Tutorial em cinco passos atualizado para o modelo idle.
- Menu com novo jogo, continuar e créditos.

## Save e migração

O formato atual é a versão 5. Ele registra progressão, tempo aberto, fração do
ciclo automático, equipe, automação, meta, tarefa e decisão pendente. Saves das
versões anteriores preservam cargo, XP e atributos; o calendário é migrado para
o Dia 1 da nova jornada idle. Nenhuma data real é salva ou usada para conceder
produção offline.

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

- 18 testes de Edit Mode para economia, relógio, contratação, automação,
  tarefas, meta, progressão completa, decisões, saves, migração e cenas.
- 9 testes de Play Mode para movimentação, estações, HUD idle, menu, tutorial,
  meta, promoção, conversa de carreira e sequência final.

## Próximas fronteiras

1. Sessões reais de playtest para ajustar custos e frequência das intervenções.
2. Mais tarefas, conversas e variações sonoras específicas por escritório.
3. Arte própria para ícone e identidade visual externa do produto.
4. Preparação do executável apenas na etapa final do projeto.
