# ByteForge — Dev Career

Jogo 2D desenvolvido em Unity no qual Alex começa como estagiário e evolui por
sete cargos até se tornar dono da ByteForge.

## Progressão

`Estagiário → Dev Júnior → Dev Pleno → Dev Sênior → Tech Lead → CTO → Dono`

O jogador conclui tarefas durante o expediente, administra energia e estresse,
desenvolve conhecimento e reputação e precisa atingir metas diárias para receber
XP adicional. Todo ponto de entrega também gera XP permanente para receber
promoções. Cada faixa de carreira possui um escritório próprio. Ao chegar ao
cargo de Dono, o jogo encerra com uma sequência automática.

## Requisitos

- Unity `6000.3.22f1`.
- Pacote de arte **Modern Interiors**, adquirido separadamente.

O pacote Modern Interiors não está incluído neste repositório por ser conteúdo
licenciado. Depois de obtê-lo, coloque os arquivos importados em:

```text
Assets/Art/ModernInteriors16/
```

Os cenários originais dos escritórios, scripts, interface e configurações do
projeto estão incluídos no repositório.

## Música

O jogo utiliza **Level Music Loop — Tigrun**, de qubodup, em loop contínuo e
volume reduzido. A faixa foi publicada sob CC0 1.0 no OpenGameArt. Os links da
fonte, licença e hash do arquivo estão em
[`Assets/Resources/Audio/CREDITS.md`](Assets/Resources/Audio/CREDITS.md).

## Como executar

1. Clone o repositório.
2. Abra a pasta do projeto na versão indicada da Unity.
3. Importe o Modern Interiors no caminho acima.
4. Abra `Assets/Scenes/MainMenu.unity`.
5. Pressione **Play**.

Se for necessário reconstruir as cenas, use o menu:

```text
Projeto → Construir MVP carreira dev
```

## Gameplay

- Na tela inicial, selecione **Jogar** para iniciar ou **Créditos** para ver a
  equipe e as atribuições de arte.
- Clique no chão para movimentar Alex.
- Clique nas estações para programar, participar de reuniões, estudar, tomar
  café ou fazer uma pausa.
- No primeiro expediente, siga o tutorial em quatro passos ou selecione
  **Pular tutorial**.
- Complete a tarefa extra exibida no cartão da sprint para receber XP bônus.
- Eventos periódicos podem alterar conhecimento, reputação, estresse ou caixa.
- Alcance a meta de entrega e encerre o dia para avançar na carreira.
- A entrega diária zera no dia seguinte, mas o XP de carreira nunca é perdido.
- Use os botões do HUD para salvar ou carregar o progresso.

## Cenas principais

- `MainMenu`: tela inicial e créditos.
- `OfficeInternMVP`: Estagiário.
- `OfficeDeveloperMVP`: Dev Júnior e Dev Pleno.
- `OfficeLeadershipMVP`: Dev Sênior e Tech Lead.
- `OfficeCtoMVP`: CTO.
- `OwnerFinale`: encerramento como Dono.

Mais informações estão em [`Docs/MVP_STATUS.md`](Docs/MVP_STATUS.md).
