# Checkpoint — redesign completo da interface

Última atualização: 26/08/2026, após a primeira etapa do novo HUD.

## Objetivo aprovado

Evoluir toda a interface do Dev Carrer Idle Game, substituindo a composição de
retângulos simples por um sistema visual coeso, legível e com identidade própria.
O escopo inclui tela inicial, créditos, HUD da carreira, gestão idle, tutorial,
decisões, configurações e tela final. O jogo continua em português, para PC e
mouse, e nenhum executável deve ser gerado nesta etapa.

## Direção visual aprovada

- Painéis arredondados, com profundidade, sombras suaves e gradientes discretos.
- Hierarquia tipográfica mais clara e áreas maiores para impedir textos cortados.
- Paleta tecnológica em azul-marinho, ciano, teal e âmbar, com coral para alertas.
- Pixel art integrada como parte da identidade, não apenas como decoração isolada.
- Microinterações nos botões, respeitando a opção de movimento reduzido.
- Resolução de referência: 1280 × 720.

## Trabalho já realizado

- Três artes próprias foram geradas com o recurso `imagegen` e copiadas para:
  - `Assets/Art/UI/Pixel/menu-developer-workstation.png`
  - `Assets/Art/UI/Pixel/tech-ornaments-sheet.png`
  - `Assets/Art/UI/Pixel/career-icon-atlas.png`
- Foi criado `Assets/Editor/UiPixelArtImporter.cs` para importar essas imagens
  com filtro Point, sem compressão, sem mipmaps e preservando transparência.
- O kit visual foi separado em três componentes serializáveis:
  - `Assets/Scripts/Runtime/UiRoundedGraphic.cs`;
  - `Assets/Scripts/Runtime/UiVerticalGradient.cs`;
  - `Assets/Scripts/Runtime/UiButtonMotion.cs`.
- Nenhum commit do redesign foi criado ainda.

## Atualização de 26/08 — interface e progressão concluídas

- O HUD da carreira foi recomposto em três zonas sem sobreposição:
  - carreira, desempenho e guia das estações à esquerda;
  - sprint e gestão idle à direita;
  - status, meta, salvar e carregar na barra inferior central.
- O atlas de carreira foi integrado ao HUD por recortes 3 × 2.
- Painéis, chips, barras e botões agora usam cantos arredondados, gradientes,
  sombras suaves e feedback de hover/clique.
- O painel idle foi alinhado à nova coluna direita e as configurações receberam
  o mesmo sistema visual.
- O tutorial foi revisto depois da inspeção em execução; título e corpo não se
  sobrepõem mais.
- `UiRoundedGraphic`, `UiVerticalGradient` e `UiButtonMotion` foram separados em
  arquivos próprios para que o Unity serialize os componentes corretamente.
- As seis cenas foram reconstruídas em batch mode sem erro de compilação.
- O menu principal recebeu composição assimétrica: marca e ações à esquerda,
  workstation em pixel art à direita e progressão resumida já na entrada.
- Créditos, decisões de carreira e final da jornada foram alinhados ao mesmo kit
  visual e inspecionados em Play Mode na resolução real do Game View.
- A economia agora comunica uma progressão real durante o expediente:
  - Estagiário começa com R$ 120 e pode comprar o Codex por R$ 120;
  - Dev Júnior pode ativar os multiagentes por R$ 250;
  - contratação fica bloqueada até Dev Pleno;
  - as missões exigem a primeira contratação no Pleno e quatro colegas no CTO;
  - depois vêm testes inteligentes, pipeline orquestrado e plataforma autônoma.
- Saves foram migrados para a versão 7. Estados antigos incompatíveis são
  limitados ao máximo do cargo, impedindo equipe no estágio e no nível Júnior.
- Revisão visual registrada em:
  - `Assets/Screenshots/hud-redesign-final.png`
  - `Assets/Screenshots/hud-redesign-tutorial.png`
- O objetivo paralelo de pontos foi substituído por 18 missões em checklist,
  com briefing, história e recompensa próprias.
- Foram adicionados pausa, replay do tutorial, autosave, ambiência, sons por
  atividade e configurações de áudio, vídeo e acessibilidade.
- Testes aprovados após a reconstrução final:
  - 17/17 EditMode;
  - 11/11 PlayMode.

## Atualização de interação nas estações

- O losango ficava fora do collider de algumas mesas; clicar exatamente no
  marcador podia não selecionar a atividade. Cada estação agora cria um collider
  próprio e amplo que cobre o losango e o botão da ação.
- Os marcadores receberam o rótulo `CLIQUE • AÇÃO`, uma linha que os associa à
  estação e destaque de escala, cor e contraste ao passar o mouse.
- O clique agora procura a estação mais próxima dentro de uma pequena tolerância,
  e o destino escolhido recebe um marcador maior de confirmação.
- O texto de status e o tutorial v3 explicam diretamente onde clicar.
- Foi adicionado um teste PlayMode que valida as cinco áreas clicáveis, incluindo
  tamanho, cobertura do losango e estado de hover.

## Próximo ponto de retomada

Fazer o playtest humano completo de leitura, sons e ritmo. Não gerar executável
até a aprovação dessa rodada.

## Estado Git neste checkpoint

A branch `main` estava limpa antes do redesign e já continha os commits:

- `7ca12ce feat: transforma carreira em idle hibrido`
- `2789b50 docs: atualiza visao e status da versao idle`

Arquivos-base do redesign ainda não rastreados:

- `Assets/Art/UI/`
- `Assets/Editor/UiPixelArtImporter.cs`
- `Assets/Scripts/Runtime/UiRoundedGraphic.cs`
- `Assets/Scripts/Runtime/UiVerticalGradient.cs`
- `Assets/Scripts/Runtime/UiButtonMotion.cs`

Este próprio checkpoint também ficará pendente até o commit final do redesign.
