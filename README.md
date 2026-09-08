# Dev Career Idle Game

Jogo 2D idle híbrido desenvolvido em Unity pelo estúdio **Dev Murilo Lima**.
Alex começa a manhã como estagiário da ByteForge e, ao longo de um expediente
de aproximadamente sete a oito horas reais, evolui até se tornar Dono.

## Ciclo principal

`Estagiário → Dev Júnior → Dev Pleno → Dev Sênior → Tech Lead → CTO → Dono`

- Alex e sua equipe geram entrega e XP automaticamente a cada 15 segundos.
- A produção só avança enquanto o jogo está aberto; não há progresso offline.
- O jogador acelera a carreira usando as estações, contratando colegas e
  melhorando o pipeline de automação.
- São 18 missões principais em checklist, três por cargo antes de Dono, sem
  prazo, falha ou opção errada.
- Estagiário aprende as ações e compra o Codex; Júnior libera multiagentes;
  contratação começa apenas em Dev Pleno.
- O save mantém tempo aberto, missão, equipe, automação, atributos e
  decisões pendentes sem acrescentar produção pelo tempo em que o jogo fechou.
- Ao alcançar Dono, a jornada termina na sequência final da ByteForge.

## Requisitos

- Unity `6000.3.22f1`.

Os personagens usam o pacote gratuito **Free TopDown RPG Retro Sprites!**, da
**ELV Games**, incluído no projeto. Os assets importados ficam em:

```text
Assets/Art/ELVCharacters/
```

A fonte, a licença original e o mapeamento dos personagens estão em
[Créditos dos personagens](Assets/Art/ELVCharacters/CREDITS.md).

## Como executar

1. Abra a pasta do projeto no Unity `6000.3.22f1`.
2. Aguarde o Unity reconstruir a pasta `Library/` na primeira abertura.
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
- Siga o losango dourado e o checklist; cada atividade usa uma única ação.
- A produção dos agentes é bônus: nenhuma missão exige esperar um ciclo terminar.
- Use **Contratar** a partir de Dev Pleno e melhore o Codex/pipeline conforme o cargo.
- Conclua a missão quando todos os itens do checklist estiverem marcados.
- Salve ou carregue a carreira pela barra inferior.
- Pressione **Esc** para pausar; a produção idle continua durante a pausa.
- Música, efeitos, ambiente, vídeo e acessibilidade ficam nas configurações.

## Cenas

- `MainMenu`: novo jogo, continuar e créditos.
- `OfficeInternMVP`: Estagiário.
- `OfficeDeveloperMVP`: Dev Júnior e Dev Pleno.
- `OfficeLeadershipMVP`: Dev Sênior e Tech Lead.
- `OfficeCtoMVP`: CTO.
- `OwnerFinale`: conclusão como Dono.

## Identidade

- Produto: **Dev Career Idle Game**
- Estúdio: **Dev Murilo Lima**
- Versão: `0.2.0`
- Identificador: `dev.murilolima.devcareer`
- Idioma inicial: português

## Música e arte

O jogo utiliza **Level Music Loop — Tigrun**, de qubodup, sob CC0 1.0. Fonte,
licença e hash estão em `Assets/Resources/Audio/CREDITS.md`. Os personagens usam
Fantasy Dreamland, da ELV Games, conforme a licença incluída no pacote gratuito.

O design completo está em `Docs/GDD.md`. O status técnico e o balanceamento
estão em `Docs/MVP_STATUS.md`.

A documentação dos fundos, com prompts de referência e o modelo informado,
está em [Prompts dos cenários](Docs/PROMPTS_CENARIOS.md).
