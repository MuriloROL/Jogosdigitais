# MVP — Dev Career: ByteForge

## Conceito atual

O jogador controla Alex, um desenvolvedor que começa como **Estagiário** na
ByteForge. O objetivo é concluir dias produtivos e subir na carreira até se tornar
o **Dono** da empresa.

Este conceito substitui o antigo simulador de bairro. A carreira começa em
`Assets/Scenes/OfficeInternMVP.unity` e troca automaticamente de ambiente nas
promoções importantes.

## Ciclo jogável

1. O expediente começa às 09:00 e possui oito horas disponíveis.
2. O jogador clica em uma estação do escritório e Alex caminha até ela.
3. Cada atividade consome tempo e altera entrega, energia, estresse,
   conhecimento ou reputação.
4. Ao alcançar a meta de entrega, o dia é considerado produtivo.
5. O botão **Encerrar dia** paga o salário, concede bônus por produtividade e
   avança o calendário.
6. A quantidade necessária de dias produtivos promove o jogador ao próximo
   cargo.

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

| Cargo | Dias produtivos para promoção | Salário mensal |
|---|---:|---:|
| Estagiário | 2 | R$ 1.800 |
| Dev Júnior | 3 | R$ 3.500 |
| Dev Pleno | 4 | R$ 6.500 |
| Dev Sênior | 5 | R$ 10.000 |
| Tech Lead | 5 | R$ 14.000 |
| CTO | 6 | R$ 22.000 |
| Dono | Cargo final | R$ 50.000 |

A meta diária cresce a cada promoção. Um dia abaixo da meta avança o calendário
e paga o valor diário, mas não conta para a promoção. Dias produtivos pagam um
bônus de 20%.

## Sistemas implementados

- Quatro escritórios top-down em pixel art detalhada, com identidade visual
  contínua e evolução clara de espaço, tecnologia e prestígio.
- Cinco pontos interativos discretos e alinhados aos móveis de cada cenário.
- Troca automática de cena conforme a faixa de cargo.
- Carreira persistente entre cenas e roteamento correto após carregar um save.
- Personagem clicável com caminhada e animações direcionais.
- Expediente de oito horas e ações com durações diferentes.
- Entrega diária, energia, estresse, conhecimento e reputação.
- Sete cargos, metas crescentes, salários e promoções por dias produtivos.
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
- Dias improdutivos não contam para promoção.
- Programação consome tempo e energia e produz entrega.
- Save/load restaura o estado completo da carreira.
- Em PlayMode, Alex caminha até a mesa e conclui uma tarefa.
- Café recupera energia e o botão de encerrar dia avança o calendário.
- Mapeamento de todos os cargos para seus respectivos ambientes.
- Promoção de Estagiário para Júnior carrega o escritório maior.
- Cena final chega ao estado concluído e não contém controles de gameplay.
- Suíte atual: 17 testes de Edit Mode e 6 testes de Play Mode aprovados.

## Próximas melhorias recomendadas

1. Adicionar tarefas sorteadas (bugs, features e incidentes) com dificuldade e
   prazo próprios.
2. Criar diálogos e escolhas durante reuniões com colegas e gestores.
3. Adicionar eventos de carreira: entrevista, feedback, crise em produção e
   negociação salarial.
4. Incluir sons de escritório, música e efeitos específicos de cada estação.
5. Acrescentar uma tela de créditos após a conclusão da cena do Dono.
