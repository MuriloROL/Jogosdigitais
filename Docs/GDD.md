# Game Design Document — Dev Career Idle Game

## 1. Controle do documento

| Campo | Valor |
|---|---|
| Produto | **Dev Career Idle Game** |
| Estúdio | **Dev Murilo Lima** |
| Gênero | Idle híbrido, simulação leve de carreira e gestão |
| Plataforma-alvo | PC, com mouse |
| Engine | Unity `6000.3.22f1` |
| Idioma | Português brasileiro |
| Versão configurada do produto | `0.2.0` |
| Identificador | `dev.murilolima.devcareer` |
| Estado | MVP funcional / candidato a playtest final |
| Atualização deste GDD | 31/08/2026 |

Este documento descreve o jogo implementado e a direção aprovada para seu
fechamento. Quando existe diferença entre intenção e implementação, ela é
registrada na seção **Decisões pendentes**, em vez de ser tratada como fato.

---

## 2. Visão geral

### 2.1 High concept

**Dev Career Idle Game** é um jogo 2D em pixel art sobre viver uma carreira
inteira em tecnologia durante um único expediente simbólico. Alex começa como
estagiário da ByteForge, executa tarefas manualmente, adota ferramentas de
automação, forma uma equipe, assume responsabilidades de liderança e termina a
jornada como Dono da empresa.

O jogo combina interação direta com produção automática. O jogador nunca é
obrigado a esperar um ciclo idle para avançar uma missão: as ações nas estações,
as compras e as contratações resolvem todos os objetivos principais de forma
imediata. A automação funciona como bônus contínuo de entrega, XP e dinheiro.

### 2.2 Proposta de valor

- Representar crescimento profissional por mudanças mecânicas e visuais.
- Transformar conceitos de tecnologia — revisão, testes, pipeline, mentoria e
  liderança — em objetivos simples e legíveis.
- Oferecer progressão idle sem progresso offline e sem espera obrigatória.
- Contar uma história positiva, sem falha permanente ou escolhas punitivas.
- Concluir uma carreira completa em uma experiência autocontida.

### 2.3 Fantasia do jogador

> “Comecei executando tickets sozinho, aprendi a trabalhar com ferramentas e
> pessoas, construí uma organização capaz de crescer e terminei liderando a
> empresa que ajudei a criar.”

---

## 3. Público e posicionamento

### 3.1 Público principal

- Pessoas interessadas em tecnologia, desenvolvimento de software e cultura de
  trabalho.
- Jogadores casuais que preferem progressão clara e baixa punição.
- Fãs de jogos idle que desejam participação ativa, narrativa e conclusão.
- Estudantes ou profissionais iniciantes que reconheçam situações de carreira.

### 3.2 Experiência desejada

- Curiosidade ao descobrir o próximo cargo e escritório.
- Satisfação ao preencher checklists curtos.
- Sensação de escala ao trocar esforço individual por automação e equipe.
- Segurança para experimentar escolhas sem medo de perder a carreira.
- Encerramento emocional claro ao chegar a Dono.

### 3.3 Classificação e conteúdo

O jogo não contém violência gráfica, apostas, compras reais ou conteúdo sexual.
Os conflitos são profissionais e tratados com humor leve. A classificação
indicativa formal ainda precisa ser definida para a distribuição.

---

## 4. Pilares de design

### 4.1 Progresso constante

Sempre existe um objetivo próximo: uma ação, item de checklist, compra,
contratação, missão ou promoção.

### 4.2 Idle com presença

A produção automática ocorre somente com o jogo aberto. O jogador pode acelerar
e direcionar a jornada por ações manuais, gestão e decisões.

### 4.3 Carreira sem espera obrigatória

Nenhuma missão exige aguardar produção automática. Se um objetivo demanda uma
compra, a economia da carreira deve fornecer recursos suficientes pelas missões
e ações; o idle permanece como margem e recompensa.

### 4.4 Escolhas positivas

As decisões alteram a ênfase entre dinheiro, XP, conhecimento, reputação e
bem-estar. Não existe resposta errada, game over ou perda definitiva.

### 4.5 Repetição legível

As cinco ações principais permanecem familiares, enquanto contexto, escala,
escritório, equipe e automação evoluem.

---

## 5. Estrutura da experiência

### 5.1 Macrofluxo

```text
Menu principal
      ↓
Nova carreira / Continuar
      ↓
Briefing da missão
      ↓
Executar ações, comprar ferramentas ou contratar
      ↓
Checklist completo → receber recompensa
      ↓
3 missões no cargo → promoção → decisão de carreira
      ↓
Novo cargo e, em marcos específicos, novo escritório
      ↓
CTO concluído → Dono → sequência final
```

### 5.2 Loop principal

1. Ler o briefing e identificar o próximo item do checklist.
2. Seguir o destaque dourado até a estação ou usar o painel de gestão.
3. Clicar na atividade e aguardar Alex chegar ao ponto de interação.
4. Receber alteração de recursos e feedback audiovisual.
5. Repetir até concluir os três itens da missão.
6. Acionar **Concluir missão**, receber dinheiro e XP e abrir o próximo briefing.

Em paralelo, um ciclo idle a cada 15 segundos gera entrega, XP e dinheiro.

### 5.3 Ritmo

- Cada cargo antes de Dono possui três missões.
- Cada missão possui três objetivos.
- As interações têm feedback curto; a estação usa aproximadamente 0,65 segundo
  de animação, ou 0,08 segundo com movimento reduzido.
- O relógio de interface representa um expediente das 09:00 às 17:00.
- Na implementação atual, oito horas do relógio equivalem a quatro horas reais.
- O relógio não impõe falha, prazo ou encerramento automático da carreira.

---

## 6. Controles e interação

| Entrada | Resultado |
|---|---|
| Mouse sobre uma estação | Destaca sua área clicável e exibe a ação |
| Clique esquerdo na estação | Define a estação como destino e executa a ação ao chegar |
| Clique esquerdo no chão | Move Alex livremente dentro dos limites do escritório |
| Clique nos painéis | Conclui missão, compra ferramenta, contrata, salva ou configura |
| `Esc` | Abre ou fecha o menu de pausa |

Não existe movimentação por teclado no escopo atual. Durante a interação com uma
estação, novos comandos de movimento são temporariamente ignorados.

### 6.1 Comunicação espacial

- Cada estação possui losango, linha de conexão e botão `CLIQUE • AÇÃO`.
- O próximo objetivo aplicável recebe destaque dourado.
- Hover aumenta contraste e escala do marcador.
- Um marcador ciano confirma o destino escolhido.
- A área clicável cobre o marcador e o rótulo, com tolerância adicional ao
  redor do ponteiro.

---

## 7. Sistemas de jogo

### 7.1 Recursos

| Recurso | Função | Origem principal |
|---|---|---|
| Entrega | Mede produção acumulada no cargo | Programação, reunião, estudo e idle |
| XP de carreira | Controla elegibilidade para promoção | Entrega manual, idle, missões e decisões |
| Dinheiro | Compra automação e contrata colegas | Recompensas, decisões e idle |
| Energia | Modifica a eficiência de programação | Recuperada com café, pausa e idle |
| Estresse | Reduz a eficiência de programação | Reduzido com café, pausa, idle e decisões |
| Conhecimento | Aumenta a produção de programação | Estudo, compras, promoções e decisões |
| Reputação | Representa influência profissional | Reuniões, missões, promoções e decisões |

Energia e estresse variam entre 0 e 100. Conhecimento e reputação nunca ficam
negativos. O jogador começa com 100 de energia, 8 de estresse e R$ 120.

### 7.2 Ações nas estações

| Ação | Efeito base implementado | Papel no design |
|---|---|---|
| Programar | Entrega variável, −18 energia, +13 estresse, +2 conhecimento, +1 reputação | Maior fonte manual de entrega |
| Reunião | +14 entrega/XP, −10 energia, +9 estresse, +7 reputação | Comunicação e liderança |
| Estudar | +5 entrega/XP, −14 energia, +5 estresse, +10 conhecimento, +1 reputação | Investimento na eficiência futura |
| Café | +30 energia, −7 estresse | Recuperação rápida |
| Pausa | +14 energia, −26 estresse | Principal controle de estresse |

#### Fórmula de programação

```text
fatorEnergia = interpolação de 0,55 a 1,00 pela energia atual
fatorEstresse = interpolação de 1,00 a 0,68 pelo estresse atual
bônusConhecimento = mínimo entre 10 e conhecimento × 0,08
entrega = arredondar(32 × fatorEnergia × fatorEstresse + bônusConhecimento)
entrega mínima = 10
XP recebido = entrega gerada
```

### 7.3 Missões e checklist

- Existem 18 missões principais: três em cada cargo de Estagiário a CTO.
- Toda missão possui briefing, três objetivos, texto de conclusão, dinheiro e XP.
- Objetivos podem exigir uma das cinco ações, um nível de ferramenta ou um
  tamanho de equipe.
- Objetivos de ação avançam uma unidade por uso da estação correspondente.
- A missão só pode ser finalizada quando todos os itens estão completos.
- Completar a terceira missão força o XP mínimo do cargo e permite promoção.
- Não há missão perdida, temporizador, resposta incorreta ou soft lock esperado.

### 7.4 Produção idle

O ciclo automático ocorre a cada 15 segundos de tempo real com o jogo aberto,
inclusive durante a pausa.

```text
entrega por ciclo = produção base da automação + número de colegas
XP por ciclo = entrega por ciclo
dinheiro por ciclo = máximo entre 1 e entrega ÷ 2 (inteiro)
energia por ciclo = +1
estresse por ciclo = −1
```

- Não existe progresso offline.
- Fechar o jogo congela o acumulador do ciclo.
- Carregar restaura o estado salvo sem simular o tempo fechado.
- Produção passiva não preenche objetivos de ação do checklist.

### 7.5 Automação

| Nível | Nome após compra | Custo | Cargo mínimo implementado | Produção-base/ciclo |
|---:|---|---:|---|---:|
| 0 | Fluxo manual | — | Estagiário | 1 |
| 1 | Codex Assistente | R$ 120 | Estagiário | 2 |
| 2 | Codex Multiagentes | R$ 250 | Dev Júnior | 3 |
| 3 | Testes inteligentes | R$ 550 | Dev Pleno | 3 |
| 4 | Pipeline orquestrado | R$ 1.000 | Dev Sênior | 4 |
| 5 | Plataforma autônoma | R$ 1.700 | Tech Lead | 5 |

Os checklists apresentam Testes inteligentes no Sênior, Pipeline orquestrado no
Tech Lead e Plataforma autônoma no CTO. Entretanto, o sistema permite comprar
cada um desses três níveis a partir do cargo anterior. Essa diferença deve ser
mantida conscientemente ou alinhada antes da versão final.

### 7.6 Equipe

A contratação é liberada em Dev Pleno.

```text
custo do próximo colega = R$ 300 × posição do colega na equipe
```

| Colega contratado | Custo |
|---:|---:|
| 1º | R$ 300 |
| 2º | R$ 600 |
| 3º | R$ 900 |
| 4º | R$ 1.200 |
| 5º | R$ 1.500 |
| 6º | R$ 1.800 |

Cada colega acrescenta +1 à produção por ciclo. A capacidade depende do cargo:

| Cargo | Capacidade da equipe |
|---|---:|
| Estagiário | 0 |
| Dev Júnior | 0 |
| Dev Pleno | 1 |
| Dev Sênior | 2 |
| Tech Lead | 4 |
| CTO | 6 |
| Dono | 6 |

### 7.7 Promoção

Uma promoção exige simultaneamente:

1. conclusão das três missões do cargo;
2. XP igual ou superior ao limite do cargo.

Ao promover, o jogador recebe bônus financeiro proporcional ao salário do novo
cargo, +10 reputação e +8 conhecimento. A entrega do cargo é zerada, uma decisão
de carreira é aberta e a cena correta é carregada quando necessário.

| Cargo | Salário exibido | XP acumulado para avançar | Escritório |
|---|---:|---:|---|
| Estagiário | R$ 1.800 | 350 | Estágio |
| Dev Júnior | R$ 3.500 | 1.100 | Desenvolvimento |
| Dev Pleno | R$ 6.500 | 2.200 | Desenvolvimento |
| Dev Sênior | R$ 10.000 | 3.800 | Liderança |
| Tech Lead | R$ 14.000 | 6.000 | Liderança |
| CTO | R$ 22.000 | 9.000 | Executivo |
| Dono | R$ 50.000 | Final | Sequência final |

O salário é um indicador de progressão e base para o bônus de promoção. Não há
pagamento periódico de salário implementado.

### 7.8 Decisões de carreira

Há uma conversa após cada promoção entre Dev Júnior e CTO. Todas as opções são
positivas e alteram recursos diferentes.

| Entrada no cargo | Situação | Opções |
|---|---|---|
| Dev Júnior | API herdada instável | Refatorar a base / Criar proteção rápida |
| Dev Pleno | Sprint apertada | Reduzir o escopo / Fazer um mutirão sustentável |
| Dev Sênior | Feedback do cliente | Pesquisar com usuários / Prototipar agora |
| Tech Lead | Escala do time | Mentoria para o time / Ferramentas de entrega |
| CTO | Parceria e dados internos | Proteger os dados / Negociar parceria segura |

As recompensas possíveis incluem dinheiro, conhecimento, reputação, redução de
estresse e XP. A decisão nunca impede a progressão.

---

## 8. Conteúdo da carreira

### 8.1 Estagiário — onboarding e primeira automação

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Primeira Manhã | Estudar 1×, reunir 1×, programar 2× | R$ 40 / 55 XP |
| Bug de Boas-vindas | Programar 3×, café 1×, pausa 1× | R$ 50 / 65 XP |
| Codex Lado a Lado | Codex nível 1, estudar 1×, programar 2× | R$ 85 / 80 XP |

Objetivo narrativo: aprender as cinco estações, compreender o checklist e
adquirir o Codex Assistente com o orçamento inicial.

### 8.2 Dev Júnior — projetos reais e multiagentes

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Primeiro Projeto Real | Codex nível 2, estudar 2×, programar 3× | R$ 80 / 90 XP |
| O Primeiro Pull Request | Programar 3×, reunir 2×, estudar 2× | R$ 90 / 100 XP |
| Deploy Acompanhado | Programar 2×, café 1×, reunir 2× | R$ 100 / 115 XP |

Objetivo narrativo: sair do ambiente de treinamento, trabalhar com usuários e
aprender revisão e deploy acompanhado.

### 8.3 Dev Pleno — responsabilidade compartilhada

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Ninguém Entrega Sozinho | Equipe 1, reunir 2×, programar 2× | R$ 150 / 130 XP |
| Dívida Técnica Tem Juros | Estudar 2×, reunir 2×, programar 3× | R$ 170 / 145 XP |
| Sprint Sustentável | Pausa 1×, programar 3×, reunir 2× | R$ 190 / 160 XP |

Objetivo narrativo: contratar o primeiro colega e trocar produtividade
individual por responsabilidade coletiva e sustentabilidade.

### 8.4 Dev Sênior — qualidade, arquitetura e mentoria

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Testes Antes do Incidente | Automação nível 3, estudar 2×, programar 3× | R$ 230 / 180 XP |
| Arquitetura sem Torre de Marfim | Programar 3×, reunir 2×, estudar 2× | R$ 250 / 195 XP |
| Mentoria que Multiplica | Reunir 2×, estudar 2×, programar 2× | R$ 270 / 210 XP |

Objetivo narrativo: ampliar impacto por qualidade, decisões explicáveis e
desenvolvimento de outras pessoas.

### 8.5 Tech Lead — sistema de entrega e cultura

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Pipeline sem Heróis | Automação nível 4, estudar 2×, reunir 2× | R$ 330 / 230 XP |
| Um Time, Vários Contextos | Equipe 2, reunir 2×, estudar 2× | R$ 380 / 250 XP |
| Incidente sem Culpados | Programar 3×, pausa 1×, reunir 2× | R$ 750 / 280 XP |

Objetivo narrativo: criar sistemas reproduzíveis, melhorar comunicação e tratar
incidentes como aprendizado em vez de culpa.

### 8.6 CTO — plataforma e sucessão

| Missão | Objetivos resumidos | Recompensa |
|---|---|---:|
| Plataforma Autônoma | Automação nível 5, reunir 2×, programar 3× | R$ 520 / 320 XP |
| Tecnologia com Contexto | Estudar 3×, reunir 2×, programar 3× | R$ 1.700 / 350 XP |
| Preparar o Próximo Capítulo | Equipe 4, reunir 3×, estudar 3× | R$ 700 / 400 XP |

Objetivo narrativo: conectar tecnologia ao negócio, preparar sucessão e tornar a
empresa menos dependente de uma única pessoa.

### 8.7 Dono — encerramento

Ao concluir CTO, Alex entra automaticamente na cena final. Não existem controles
de escritório nessa sequência. Alex caminha pelo salão da ByteForge, aproxima-se
do trono simbólico, recebe celebração com confetes e encerra a jornada com a
mensagem **“Você construiu a ByteForge”**.

A sequência conclui em até oito segundos. Com movimento reduzido, ela apresenta
o estado final em aproximadamente meio segundo.

---

## 9. Mundo, cenas e progressão visual

| Cena | Função | Cargos |
|---|---|---|
| `MainMenu` | Novo jogo, continuar e créditos | — |
| `OfficeInternMVP` | Escritório inicial compacto | Estagiário |
| `OfficeDeveloperMVP` | Escritório de desenvolvimento | Dev Júnior e Dev Pleno |
| `OfficeLeadershipMVP` | Ambiente de liderança | Dev Sênior e Tech Lead |
| `OfficeCtoMVP` | Escritório executivo | CTO |
| `OwnerFinale` | Encerramento sem controle | Dono |

Os escritórios ficam maiores e mais prestigiosos conforme a carreira avança.
Todos mantêm as mesmas cinco estações para preservar legibilidade do loop.

---

## 10. Interface e experiência do usuário

### 10.1 Menu principal

- Marca e ações à esquerda.
- Ilustração de workstation à direita.
- Botões **Jogar**, **Continuar** e **Créditos**.
- **Continuar** depende da existência de save válido.
- Novo jogo inicia uma carreira limpa no escritório de Estagiário.

### 10.2 HUD da carreira

Resolução de referência: 1280 × 720.

- Coluna esquerda: cargo, XP, desempenho e guia das estações.
- Centro superior: expediente, horário, salário e caixa.
- Coluna direita: missão atual, checklist e gestão idle.
- Barra inferior: status da interação, objetivo, salvar e carregar.
- Briefings e decisões aparecem como overlays modais.

### 10.3 Hierarquia de orientação

1. Briefing explica o contexto.
2. Checklist informa a ação necessária.
3. Losango dourado mostra onde agir.
4. Hover confirma a estação.
5. Status inferior informa movimento, ação e resultado.
6. Som e pulsos reforçam ganho ou conclusão.

### 10.4 Pausa

O menu aberto com `Esc` permite:

- continuar;
- salvar imediatamente;
- rever o tutorial;
- voltar ao menu principal.

O mundo usa `timeScale = 0`, mas a produção idle continua por tempo não escalado.

---

## 11. Tutorial e acessibilidade

### 11.1 Tutorial inicial

O tutorial possui cinco passos:

1. clicar e caminhar até as estações;
2. seguir checklist e marcador dourado;
3. comprar Codex, liberar multiagentes e contratar no Pleno;
4. administrar energia e estresse;
5. entender missões, escolhas positivas e autosave.

O tutorial pode ser ignorado e revisto pelo menu de pausa.

### 11.2 Recursos de acessibilidade

- Movimento reduzido.
- Texto ampliado.
- Alto contraste.
- Controle independente de música, efeitos e ambiente.
- Alternância entre tela cheia e janela.
- Seleção de resolução.
- Seleção de qualidade gráfica.

Movimento reduzido remove ou encurta pulsos, microinterações e a animação final.
Texto ampliado altera a resolução de referência do Canvas. Alto contraste ajusta
cores de textos e contornos.

---

## 12. Direção de arte

### 12.1 Estilo

- Pixel art top-down.
- Ambientes de escritório com leitura imediata das estações.
- Paleta tecnológica em azul-marinho, ciano, teal e âmbar.
- Coral reservado para alertas.
- Painéis arredondados, sombras suaves e gradientes discretos.
- Ícones de carreira e ornamentos integrados ao HUD.

### 12.2 Assets

- Personagens: **Free TopDown RPG Retro Sprites! / Fantasy Dreamland**, da ELV Games.
- Escritórios autorais gerados para os quatro níveis visuais.
- Kit visual próprio:
  - `menu-developer-workstation.png`;
  - `tech-ornaments-sheet.png`;
  - `career-icon-atlas.png`.

Os prompts de referência para os fundos e o modelo informado pelo responsável,
GPT 6 Astra da OpenAI, estão em [Prompts dos cenários](PROMPTS_CENARIOS.md).

Os sprites de personagens, a licença original e os créditos estão em
[`Assets/Art/ELVCharacters/`](../Assets/Art/ELVCharacters/CREDITS.md).
O histórico Git ainda contém o pacote anterior; a substituição dos arquivos
atuais não remove os assets antigos dos commits já existentes.

### 12.3 Animação

- Alex usa caminhada em quatro direções, com passos sincronizados à distância.
- Alex e os colegas usam animação parado; movimento reduzido mantém a pose estática.
- Estações pulsam durante interação.
- Marcadores reagem ao hover e ao objetivo da missão.
- Botões possuem feedback de hover e clique.
- A cena final utiliza caminhada, aura, escala e confetes.

---

## 13. Direção de áudio

### 13.1 Música

- Faixa: **Level Music Loop — Tigrun**, de qubodup.
- Licença: CC0 1.0 Universal.
- Reprodução contínua em loop.

### 13.2 Efeitos

Há feedback específico para:

- programação;
- reunião;
- estudo;
- café;
- pausa;
- conclusão de missão;
- promoção;
- decisão.

### 13.3 Ambiente

Uma camada de ambiência de escritório complementa música e efeitos. Música,
efeitos e ambiente possuem volumes independentes e persistentes.

---

## 14. Save e persistência

### 14.1 Comportamento

- Save manual pela HUD e pelo menu de pausa.
- Load manual pela HUD e pelo menu principal.
- Autosave depois de ação, compra, contratação, missão, promoção e decisão.
- Arquivo padrão: `dev-career-mvp.json` em `Application.persistentDataPath`.
- Formato atual: versão 7.

### 14.2 Dados persistidos

- cargo e missão;
- progresso do checklist;
- dinheiro, entrega, energia, estresse, conhecimento, reputação e XP;
- tempo com o jogo aberto e acumulador idle;
- equipe e automação;
- decisão pendente;
- estado do briefing;
- eventos e mensagens recentes.

### 14.3 Migração

- Saves anteriores são adaptados para a economia idle atual.
- Contratações incompatíveis com Estagiário ou Júnior são removidas.
- Automação e equipe são limitadas ao máximo do cargo.
- Nenhum progresso offline é concedido na migração.
- Saves da antiga pasta **Dev Carrer Idle Game** são encontrados e gravados na
  nova pasta **Dev Career Idle Game** após o carregamento.

Preferências de áudio, vídeo, acessibilidade e tutorial usam `PlayerPrefs`.

---

## 15. Arquitetura técnica

### 15.1 Base

- Unity `6000.3.22f1`.
- Universal Render Pipeline 2D.
- Input System para ponteiro e teclado.
- UGUI para menu, HUD e overlays.
- Assembly Definition separado para runtime, Edit Mode e Play Mode.

### 15.2 Responsabilidades principais

| Componente | Responsabilidade |
|---|---|
| `CareerGame` | Estado, ações, economia, missão, save e promoção |
| `CareerMissionCatalog` | Conteúdo das 18 missões |
| `CareerSceneFlow` | Mapeamento cargo → cena |
| `OfficePlayerController` | Ponteiro, movimento e chegada à estação |
| `OfficeStation` | Área clicável, feedback e execução de ação |
| `CareerHud` | Apresentação dos recursos e checklist |
| `IdleManagementPanel` | Contratação, automação e configurações |
| `CareerDecisionController` | Conversas e escolhas positivas |
| `CareerTutorialController` | Tutorial de cinco passos |
| `PauseMenuController` | Pausa e ações de sessão |
| `OwnerFinaleController` | Sequência final |
| `OfficeMvpSceneBuilder` | Reconstrução automatizada das cenas |

### 15.3 Restrições

- O jogo deve continuar funcional sem conexão com internet.
- O runtime não depende da integração Codex/MCP usada no editor.
- Todas as cenas jogáveis precisam permanecer no `EditorBuildSettings`.
- O projeto deve ser aberto na versão de Unity registrada para evitar migrações
  automáticas não planejadas.

---

## 16. Qualidade e testes

### 16.1 Cobertura automatizada atual

**Edit Mode — 17 testes**

- efeitos das ações;
- ciclo idle de 15 segundos;
- orçamento inicial e Codex;
- bloqueio de contratação por cargo;
- 18 missões sem espera obrigatória;
- economia da carreira completa;
- ausência de falha após o expediente;
- save, load e migração;
- mapeamento de cargos para cenas.

**Play Mode — 11 testes**

- movimento até a estação;
- efeitos de café;
- cinco marcadores clicáveis;
- conclusão de checklist pela HUD;
- animação horizontal;
- troca de cena por promoção;
- menu, créditos e novo jogo;
- tutorial;
- progressão do painel idle;
- sequência final;
- produção durante a pausa.

Última execução registrada antes deste documento: **17/17 Edit Mode e 11/11
Play Mode aprovados**.

### 16.2 Playtest humano obrigatório

O teste automatizado não cobre qualidade subjetiva. Antes da versão final, uma
carreira completa deve validar:

- clareza dos briefings e textos;
- ritmo real da progressão;
- legibilidade em diferentes resoluções;
- conforto de animações e flashes;
- equilíbrio entre volumes;
- ausência de cliques frustrados;
- percepção dos ganhos idle;
- funcionamento de save/load após reiniciar o executável;
- impacto emocional da conclusão.

---

## 17. Escopo

### 17.1 Incluído no MVP

- Carreira completa de Estagiário a Dono.
- 18 missões principais.
- Cinco estações interativas.
- Automação em cinco níveis adquiríveis.
- Contratação de até seis colegas.
- Cinco decisões de carreira.
- Quatro escritórios e uma cena final.
- Tutorial, pausa, save e configurações.
- Música, efeitos e ambiência.
- Acessibilidade visual básica.

### 17.2 Fora do escopo atual

- Progresso offline.
- Monetização, anúncios ou compras reais.
- Game over, demissão ou escolha errada.
- Conquistas.
- Filiais ou sistema de prestígio depois de Dono.
- Multiplayer.
- Localização para outros idiomas.
- Controles por teclado ou gamepad.
- Conteúdo pós-final.

### 17.3 Possibilidades pós-lançamento

Estas ideias não fazem parte da definição de pronto:

- conquistas opcionais;
- localização;
- controles alternativos;
- modo acelerado para replay;
- estatísticas finais da carreira;
- novas empresas, especializações ou ciclos de prestígio.

---

## 18. Critérios de conclusão

O MVP pode ser considerado pronto para entrega quando:

1. o nome **Dev Career Idle Game** estiver consistente em jogo e distribuição;
2. uma carreira completa puder ser concluída sem bloqueio;
3. textos não cortarem em 1280 × 720 e 1920 × 1080;
4. música, efeitos e ambiência estiverem equilibrados;
5. acessibilidade funcionar em menu, HUD, overlays e final;
6. save e continue funcionarem após fechar e reabrir o executável;
7. as 28 verificações automatizadas permanecerem verdes;
8. um playtest humano completo for aprovado;
9. o executável final for testado fora do Unity e em uma segunda máquina;
10. ícone, versão, créditos e plataforma de distribuição forem definidos.

---

## 19. Decisões pendentes e riscos

| Tema | Estado atual | Decisão necessária |
|---|---|---|
| Duração | README antigo menciona 7–8 horas; código comprime o expediente em 4 horas reais; missões podem terminar antes | Medir no playtest e definir duração oficial |
| Versão | Project Settings usa 0.2.0; cena do menu mostra 0.3.0; gerador usa 0.4.0 | Escolher uma única versão e derivar a UI dela |
| Automação | Níveis 3–5 podem ser comprados um cargo antes da missão que os apresenta | Confirmar antecipação ou alinhar desbloqueios |
| Salário | Valor é exibido, mas não existe pagamento periódico | Assumir como indicador ou implementar pagamento |
| Plataforma | PC está definido, mas o sistema operacional de lançamento não | Escolher Windows, Linux ou ambos |
| Distribuição | Ainda não existe executável final nem ícone configurado | Preparar e validar o pacote de release |
| Licenciamento | Personagens atuais usam ELV Games; o histórico Git contém assets do pacote anterior | Manter créditos e licença; revisar o histórico antes de publicar o repositório |
| Ritmo | Economia foi validada por testes, não por uma sessão humana completa | Realizar playtest de ponta a ponta |

---

## 20. Glossário

| Termo | Definição |
|---|---|
| Entrega | Unidade de produção gerada pelo trabalho manual e automático |
| XP | Progresso acumulado usado nas promoções |
| Ciclo idle | Produção automática executada a cada 15 segundos abertos |
| Automação | Evolução do fluxo manual até a plataforma autônoma |
| Checklist | Três objetivos necessários para concluir uma missão |
| Cargo | Etapa da carreira que define conteúdo, limites e ambiente |
| Briefing | Introdução narrativa exibida no início de uma missão |
| Decisão | Conversa após promoção com duas recompensas positivas |
| Movimento reduzido | Opção que minimiza pulsos, transições e animação final |

---

## 21. Referências internas

- `README.md` — execução e visão resumida.
- `Docs/GAME_VISION.md` — fantasia e pilares.
- `Docs/MVP_STATUS.md` — estado técnico do MVP.
- `Docs/HUD_REDESIGN_CHECKPOINT.md` — direção e evolução da interface.
- `Assets/Scripts/Runtime/CareerGame.cs` — regras e economia implementadas.
- `Assets/Scripts/Runtime/CareerMissionCatalog.cs` — catálogo completo de missões.
- `Assets/Tests/` — critérios automatizados de comportamento.
