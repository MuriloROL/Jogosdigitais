# ByteForge — Dev Career

Jogo 2D desenvolvido em Unity no qual Alex começa como estagiário e evolui por
sete cargos até se tornar dono da ByteForge.

## Progressão

`Estagiário → Dev Júnior → Dev Pleno → Dev Sênior → Tech Lead → CTO → Dono`

O jogador conclui tarefas durante o expediente, administra energia e estresse,
desenvolve conhecimento e reputação e precisa atingir metas diárias para receber
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

## Como executar

1. Clone o repositório.
2. Abra a pasta do projeto na versão indicada da Unity.
3. Importe o Modern Interiors no caminho acima.
4. Abra `Assets/Scenes/OfficeInternMVP.unity`.
5. Pressione **Play**.

Se for necessário reconstruir as cenas, use o menu:

```text
Projeto → Construir MVP carreira dev
```

## Gameplay

- Clique no chão para movimentar Alex.
- Clique nas estações para programar, participar de reuniões, estudar, tomar
  café ou fazer uma pausa.
- Alcance a meta de entrega e encerre o dia para avançar na carreira.
- Use os botões do HUD para salvar ou carregar o progresso.

## Cenas principais

- `OfficeInternMVP`: Estagiário.
- `OfficeDeveloperMVP`: Dev Júnior e Dev Pleno.
- `OfficeLeadershipMVP`: Dev Sênior e Tech Lead.
- `OfficeCtoMVP`: CTO.
- `OwnerFinale`: encerramento como Dono.

Mais informações estão em [`Docs/MVP_STATUS.md`](Docs/MVP_STATUS.md).
