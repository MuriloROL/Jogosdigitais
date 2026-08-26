# Dev Carrer Idle Game

Jogo 2D idle híbrido desenvolvido em Unity pelo estúdio **Dev Murilo Lima**.
Alex começa a manhã como estagiário da ByteForge e, ao longo de um expediente
de aproximadamente sete a oito horas reais, evolui até se tornar Dono.

## Ciclo principal

`Estagiário → Dev Júnior → Dev Pleno → Dev Sênior → Tech Lead → CTO → Dono`

- Alex e sua equipe geram entrega e XP automaticamente a cada 15 segundos.
- A produção só avança enquanto o jogo está aberto; não há progresso offline.
- O jogador acelera a carreira usando as estações, contratando colegas e
  melhorando o pipeline de automação.
- Cada cargo libera uma vaga adicional na equipe e apresenta uma decisão de
  carreira com duas consequências diferentes.
- Metas de sprint concedem dinheiro e XP, enquanto tarefas extras incentivam
  combinações diferentes de atividades.
- O save mantém tempo aberto, XP, equipe, automação, atributos, tarefas e
  decisões pendentes sem acrescentar produção pelo tempo em que o jogo fechou.
- Ao alcançar Dono, a jornada termina na sequência final da ByteForge.

## Requisitos

- Unity `6000.3.22f1`.
- Pacote de arte **Modern Interiors**, adquirido separadamente.

O Modern Interiors não está incluído no repositório por ser conteúdo
licenciado. Depois de obtê-lo, importe-o em:

```text
Assets/Art/ModernInteriors16/
```

## Como executar

1. Abra a pasta do projeto no Unity `6000.3.22f1`.
2. Importe o Modern Interiors no caminho indicado, caso ainda não esteja local.
3. Abra `Assets/Scenes/MainMenu.unity`.
4. Pressione **Play**.

Use **Jogar** para iniciar uma carreira nova ou **Continuar** para carregar um
save existente. Se for necessário reconstruir as cenas, use:

```text
Projeto → Construir MVP carreira dev
```

## Controles e gestão

- Clique no chão para movimentar Alex.
- Clique em uma estação para programar, reunir, estudar, tomar café ou pausar.
- Use **Contratar** para aumentar a produção passiva.
- Use **Automatizar** para melhorar a eficiência de toda a equipe.
- Resgate a meta da sprint assim que ela for concluída.
- Salve ou carregue a carreira pela barra inferior.
- Áudio, efeitos, movimento reduzido e texto ampliado ficam no painel de
  acessibilidade.

## Cenas

- `MainMenu`: novo jogo, continuar e créditos.
- `OfficeInternMVP`: Estagiário.
- `OfficeDeveloperMVP`: Dev Júnior e Dev Pleno.
- `OfficeLeadershipMVP`: Dev Sênior e Tech Lead.
- `OfficeCtoMVP`: CTO.
- `OwnerFinale`: conclusão como Dono.

## Identidade

- Produto: **Dev Carrer Idle Game**
- Estúdio: **Dev Murilo Lima**
- Versão: `0.2.0`
- Identificador: `dev.murilolima.devcarrer`
- Idioma inicial: português

## Música e arte

O jogo utiliza **Level Music Loop — Tigrun**, de qubodup, sob CC0 1.0. Fonte,
licença e hash estão em `Assets/Resources/Audio/CREDITS.md`. Os personagens usam
Modern Interiors, de LimeZu, conforme a licença do pacote adquirido.

O status técnico e o balanceamento estão em `Docs/MVP_STATUS.md`.
