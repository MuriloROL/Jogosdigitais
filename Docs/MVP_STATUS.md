# MVP — Dev Career: ByteForge

## Conceito atual

O jogador controla Alex, um desenvolvedor que começa como **Estagiário** na
ByteForge. O objetivo é concluir dias produtivos e subir na carreira até se tornar
o **Dono** da empresa.

O jogo começa em `Assets/Scenes/MainMenu.unity`; o botão **Jogar** abre a carreira em
`Assets/Scenes/OfficeInternMVP.unity`, que troca automaticamente de ambiente nas
promoções importantes. A tela inicial também possui um painel de créditos.

## Ciclo jogável

1. O expediente começa às 09:00 e possui oito horas disponíveis.
2. O jogador clica em uma estação do escritório e Alex caminha até ela.
3. Cada atividade consome tempo e altera entrega, energia, estresse,
   conhecimento ou reputação. Toda entrega também concede a mesma quantidade de
   XP permanente.
4. Ao alcançar a meta de entrega, o dia é considerado produtivo.
5. O botão **Encerrar dia** paga o salário, concede bônus por produtividade e
   avança o calendário.
6. O XP acumulado promove o jogador ao próximo cargo. A entrega diária volta a
   zero, mas o XP nunca é perdido.

## Tarefas e eventos

Cada expediente apresenta uma tarefa extra que incentiva combinações diferentes
de atividades. As tarefas incluem caça a bugs, alinhamento de sprint, estudo,
foco profundo, pausa sustentável, café com o time e reunião com stakeholders.
Concluir a tarefa concede entre 15 e 45 XP adicionais.

Em dias periódicos ocorre um evento de carreira. Workshops, incidentes em
produção, elogios de clientes e bônus de projeto alteram conhecimento, estresse,
reputação ou dinheiro e ficam registrados no feedback do expediente.

## Evolução dos escritórios

| Faixa de carreira | Cena | Ambiente |
|---|---|---|
| Estagiário | `OfficeInternMVP` | Sala compacta e acolhedora, com móveis usados, caixas, luz fluorescente e equipamentos compartilhados. |
| Dev Júnior e Dev Pleno | `OfficeDeveloperMVP` | Escritório aberto claro, várias estações, sala de vidro, biblioteca, copa e lounge. |
| Dev Sênior e Tech Lead | `OfficeLeadershipMVP` | Andar premium com war room, painéis de arquitetura, servidores, prêmios e iluminação tecnológica. |
| CTO | `OfficeCtoMVP` | Sala executiva privativa com vista noturna, madeira nobre, estratégia, espresso e lounge próprio. |
| Dono | `OwnerFinale` | Sequência final automática; não possui gameplay direto. |

Ao chegar a Dono, Alex entra no salão da empresa, percorre os marcos da carreira
e chega ao trono da ByteForge. Confetes e a mensagem final encerram o jogo.

## Estações do escritório

- **Programar:** principal fonte de pontos de entrega; consome energia e aumenta
  o estresse. Conhecimento melhora a produtividade.
- **Reunião:** gera entrega moderada e aumenta bastante a reputação.
- **Estudar:** aumenta o conhecimento permanente e gera uma pequena entrega.
- **Café:** recupera energia e reduz um pouco o estresse.
- **Pausa:** principal forma de controlar o estresse.

## Progressão de carreira

| Cargo | XP total para promoção | Salário mensal |
|---|---:|---:|
| Estagiário | 250 | R$ 1.800 |
| Dev Júnior | 700 | R$ 3.500 |
| Dev Pleno | 1.350 | R$ 6.500 |
| Dev Sênior | 2.250 | R$ 10.000 |
| Tech Lead | 3.250 | R$ 14.000 |
| CTO | 4.550 | R$ 22.000 |
| Dono | Cargo final | R$ 50.000 |

A meta diária cresce a cada promoção. Toda entrega concede a mesma quantidade de
XP, inclusive quando a meta diária não é alcançada. Dias produtivos concedem 25
XP adicionais e pagam um bônus salarial de 20%. Os limites da tabela são
cumulativos, portanto o XP não zera nem mesmo depois de uma promoção.

## Sistemas implementados

- Tela inicial ByteForge com botões para jogar e consultar os créditos.
- Tutorial inicial em quatro passos, exibido uma única vez e com opção de pular.
- Quatro escritórios top-down em pixel art detalhada, com identidade visual
  contínua e evolução clara de espaço, tecnologia e prestígio.
- Cinco pontos interativos discretos e alinhados aos móveis de cada cenário.
- Troca automática de cena conforme a faixa de cargo.
- Carreira persistente entre cenas e roteamento correto após carregar um save.
- Personagem clicável com caminhada e animações direcionais.
- Expediente de oito horas e ações com durações diferentes.
- Entrega diária, energia, estresse, conhecimento e reputação.
- Sete cargos, metas crescentes, salários e promoções por XP persistente.
- Sete tarefas diárias rotativas com objetivos e recompensas próprias.
- Quatro eventos periódicos que alteram o estado da carreira.
- Música ambiente CC0 em loop contínuo entre todas as cenas.
- Colegas decorativos e identificação dinâmica do cargo sobre o personagem.
- HUD compacto no estilo ByteForge, com cartões de carreira, expediente,
  desempenho e sprint; barras de promoção, horário, entrega, energia e estresse.
- Barra inferior de status com ações de encerrar dia, salvar e carregar sem
  esconder a área central do escritório.
- Botões para encerrar o dia, salvar e carregar a carreira.
- Save local em `dev-career-mvp.json`.
- Cena final animada sem controlador jogável para o cargo de Dono.

## Testes automatizados

- Progressão validada do Estagiário até Dono.
- A entrega diária é zerada ao encerrar o expediente, mas o XP é preservado.
- Dias abaixo da meta mantêm todo o XP obtido nas tarefas.
- Programação consome tempo e energia e produz entrega.
- Save/load restaura o estado completo da carreira.
- Em PlayMode, Alex caminha até a mesa e conclui uma tarefa.
- Café recupera energia e o botão de encerrar dia avança o calendário.
- Mapeamento de todos os cargos para seus respectivos ambientes.
- Promoção de Estagiário para Júnior carrega o escritório maior.
- Cena final chega ao estado concluído e não contém controles de gameplay.
- Tela inicial alterna corretamente entre menu e créditos, e o botão Jogar abre
  o escritório de Estagiário.
- Tutorial inicial percorre quatro passos e registra sua conclusão.
- Tarefas diárias concedem XP uma única vez e eventos alteram o novo expediente.
- Suíte atual: 16 testes de Edit Mode e 8 testes de Play Mode.

## Próximas melhorias recomendadas

1. Criar diálogos e escolhas durante reuniões com colegas e gestores.
2. Expandir os eventos com decisões do jogador e consequências de longo prazo.
3. Incluir sons de escritório, música e efeitos específicos de cada estação.
4. Adicionar configurações de áudio, vídeo e acessibilidade.
5. Gerar e validar builds distribuíveis para as plataformas-alvo.
