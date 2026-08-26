using System;

namespace ProjetoJogosDigitais.Gameplay
{
    public enum MissionObjectiveKind
    {
        Code,
        Meeting,
        Study,
        Coffee,
        Break,
        ToolLevel,
        TeamSize
    }

    public readonly struct MissionObjective
    {
        public MissionObjective(MissionObjectiveKind kind, string label, int required)
        {
            Kind = kind;
            Label = label;
            Required = required;
        }

        public MissionObjectiveKind Kind { get; }
        public string Label { get; }
        public int Required { get; }
    }

    public readonly struct CareerMissionDefinition
    {
        public CareerMissionDefinition(
            string title,
            string briefing,
            string completion,
            int rewardMoney,
            int rewardXp,
            params MissionObjective[] objectives)
        {
            Title = title;
            Briefing = briefing;
            Completion = completion;
            RewardMoney = rewardMoney;
            RewardXp = rewardXp;
            Objectives = objectives;
        }

        public string Title { get; }
        public string Briefing { get; }
        public string Completion { get; }
        public int RewardMoney { get; }
        public int RewardXp { get; }
        public MissionObjective[] Objectives { get; }
    }

    public static class CareerMissionCatalog
    {
        public const int MissionsPerRank = 3;

        private static MissionObjective O(MissionObjectiveKind kind, string label, int required)
            => new(kind, label, required);

        private static readonly CareerMissionDefinition[][] Missions =
        {
            // Estagiário — onboarding, Codex e primeira entrega.
            new[]
            {
                new CareerMissionDefinition(
                    "PRIMEIRA MANHÃ",
                    "Marta preparou um passeio pelo fluxo da ByteForge. Conheça a documentação, alinhe a tarefa e faça sua primeira entrega.",
                    "Marta sorriu: você fez perguntas antes de alterar produção. Um começo raro e muito bem-vindo.",
                    40, 55,
                    O(MissionObjectiveKind.Study, "Estudar a documentação", 1),
                    O(MissionObjectiveKind.Meeting, "Alinhar a tarefa com Marta", 1),
                    O(MissionObjectiveKind.Code, "Programar o primeiro ticket", 2)),
                new CareerMissionDefinition(
                    "BUG DE BOAS-VINDAS",
                    "Um botão desapareceu em telas pequenas. Corrija com calma e descubra por que café não substitui uma pausa.",
                    "O botão voltou, o cliente agradeceu e ninguém precisou dizer que funcionava na máquina local.",
                    50, 65,
                    O(MissionObjectiveKind.Code, "Corrigir e revisar o bug", 3),
                    O(MissionObjectiveKind.Coffee, "Recuperar energia", 1),
                    O(MissionObjectiveKind.Break, "Fazer uma pausa curta", 1)),
                new CareerMissionDefinition(
                    "CODEX LADO A LADO",
                    "A equipe liberou uma verba pequena para ferramentas. Configure o Codex, confira as sugestões e entregue junto com ele.",
                    "Seu primeiro fluxo assistido ficou pronto. O Codex ajuda; a responsabilidade pelo código continua humana.",
                    85, 80,
                    O(MissionObjectiveKind.ToolLevel, "Comprar o Codex Assistente", 1),
                    O(MissionObjectiveKind.Study, "Conferir as sugestões do Codex", 1),
                    O(MissionObjectiveKind.Code, "Revisar entregas com o Codex", 2))
            },

            // Júnior — projetos reais e multiagentes.
            new[]
            {
                new CareerMissionDefinition(
                    "PRIMEIRO PROJETO REAL",
                    "Você recebeu uma pequena API com usuários de verdade. Ative os multiagentes, estude a base e implemente com testes.",
                    "A API entrou em produção sem drama. Para a equipe, isso conta como uma ótima terça-feira.",
                    80, 90,
                    O(MissionObjectiveKind.ToolLevel, "Ativar Codex Multiagentes", 2),
                    O(MissionObjectiveKind.Study, "Estudar a base existente", 2),
                    O(MissionObjectiveKind.Code, "Implementar a solução", 3)),
                new CareerMissionDefinition(
                    "O PRIMEIRO PULL REQUEST",
                    "Seu código agora será revisado por outras pessoas. Prepare a mudança, converse sobre os comentários e valide o trabalho dos agentes.",
                    "O pull request foi aprovado. Houve sugestões, nenhuma guerra de tabs versus espaços.",
                    90, 100,
                    O(MissionObjectiveKind.Code, "Preparar o pull request", 3),
                    O(MissionObjectiveKind.Meeting, "Discutir a revisão", 2),
                    O(MissionObjectiveKind.Study, "Validar o trabalho dos agentes", 2)),
                new CareerMissionDefinition(
                    "DEPLOY ACOMPANHADO",
                    "Chegou a hora do seu primeiro deploy acompanhado. Faça a checagem final e confirme a entrega com a equipe.",
                    "Deploy concluído. O gráfico continuou verde e o canal de incidentes permaneceu silencioso.",
                    100, 115,
                    O(MissionObjectiveKind.Code, "Fazer a checagem final", 2),
                    O(MissionObjectiveKind.Coffee, "Preparar-se para o deploy", 1),
                    O(MissionObjectiveKind.Meeting, "Confirmar o deploy com a equipe", 2))
            },

            // Pleno — primeiro colega e responsabilidade compartilhada.
            new[]
            {
                new CareerMissionDefinition(
                    "NINGUÉM ENTREGA SOZINHO",
                    "Como Pleno, você pode formar sua primeira dupla. Contrate com responsabilidade e combinem a divisão do trabalho.",
                    "A primeira dupla da sua carreira começou bem: contexto compartilhado e tarefas pequenas.",
                    150, 130,
                    O(MissionObjectiveKind.TeamSize, "Contratar o primeiro colega", 1),
                    O(MissionObjectiveKind.Meeting, "Combinar a divisão do trabalho", 2),
                    O(MissionObjectiveKind.Code, "Entregar a primeira tarefa em dupla", 2)),
                new CareerMissionDefinition(
                    "DÍVIDA TÉCNICA TEM JUROS",
                    "Uma parte antiga do sistema cobra seu preço. Estude o problema, alinhe o risco e refatore em etapas.",
                    "A dívida não desapareceu, mas agora tem plano, responsáveis e muito menos juros.",
                    170, 145,
                    O(MissionObjectiveKind.Study, "Mapear a dívida técnica", 2),
                    O(MissionObjectiveKind.Meeting, "Alinhar riscos com o time", 2),
                    O(MissionObjectiveKind.Code, "Refatorar em etapas", 3)),
                new CareerMissionDefinition(
                    "SPRINT SUSTENTÁVEL",
                    "A equipe acelerou demais. Organize a entrega sem transformar horas extras em processo oficial.",
                    "A sprint terminou no prazo e com energia para a próxima. Sustentável também é produtividade.",
                    190, 160,
                    O(MissionObjectiveKind.Break, "Reduzir a pressão da equipe", 1),
                    O(MissionObjectiveKind.Code, "Concluir o escopo essencial", 3),
                    O(MissionObjectiveKind.Meeting, "Reorganizar o próximo ciclo", 2))
            },

            // Sênior — testes, arquitetura e mentoria.
            new[]
            {
                new CareerMissionDefinition(
                    "TESTES ANTES DO INCIDENTE",
                    "A cobertura atual depende bastante de esperança. Automatize os testes e fortaleça os caminhos críticos.",
                    "Os testes encontraram um erro antes dos usuários. Pela primeira vez, o vermelho foi uma boa notícia.",
                    230, 180,
                    O(MissionObjectiveKind.ToolLevel, "Ativar testes inteligentes", 3),
                    O(MissionObjectiveKind.Study, "Mapear cenários críticos", 2),
                    O(MissionObjectiveKind.Code, "Cobrir os caminhos críticos", 3)),
                new CareerMissionDefinition(
                    "ARQUITETURA SEM TORRE DE MARFIM",
                    "Uma decisão estrutural precisa funcionar no código e na rotina do time. Prototipe, explique e valide.",
                    "A arquitetura coube no diagrama e no repositório. Um feito mais raro do que parece.",
                    250, 195,
                    O(MissionObjectiveKind.Code, "Criar um protótipo técnico", 3),
                    O(MissionObjectiveKind.Meeting, "Explicar a decisão ao time", 2),
                    O(MissionObjectiveKind.Study, "Revisar os resultados do protótipo", 2)),
                new CareerMissionDefinition(
                    "MENTORIA QUE MULTIPLICA",
                    "Um colega travou em um problema que você já enfrentou. Ensine o raciocínio sem tomar o teclado.",
                    "O colega resolveu a tarefa e documentou o aprendizado. Você entregou sem escrever a solução.",
                    270, 210,
                    O(MissionObjectiveKind.Meeting, "Fazer uma sessão de mentoria", 2),
                    O(MissionObjectiveKind.Study, "Preparar material de apoio", 2),
                    O(MissionObjectiveKind.Code, "Revisar a solução do colega", 2))
            },

            // Tech Lead — pipeline, equipe e comunicação.
            new[]
            {
                new CareerMissionDefinition(
                    "PIPELINE SEM HERÓIS",
                    "Deploy não deve depender da pessoa que lembra todos os comandos. Orquestre o pipeline e documente o fluxo.",
                    "Agora qualquer pessoa preparada consegue publicar com segurança. O botão não tem o nome de ninguém.",
                    330, 230,
                    O(MissionObjectiveKind.ToolLevel, "Orquestrar o pipeline", 4),
                    O(MissionObjectiveKind.Study, "Documentar o fluxo", 2),
                    O(MissionObjectiveKind.Meeting, "Aprovar o novo pipeline", 2)),
                new CareerMissionDefinition(
                    "UM TIME, VÁRIOS CONTEXTOS",
                    "A equipe cresceu e informação começou a se perder. Amplie o time e crie acordos simples de comunicação.",
                    "As reuniões ficaram menores porque as decisões passaram a ser encontradas depois delas.",
                    380, 250,
                    O(MissionObjectiveKind.TeamSize, "Formar uma equipe com dois colegas", 2),
                    O(MissionObjectiveKind.Meeting, "Definir acordos de comunicação", 2),
                    O(MissionObjectiveKind.Study, "Documentar o novo fluxo", 2)),
                new CareerMissionDefinition(
                    "INCIDENTE SEM CULPADOS",
                    "Um incidente atingiu clientes. Coordene a correção, preserve o time e transforme o susto em aprendizado.",
                    "O serviço voltou e a retrospectiva gerou ações concretas, não uma caça a culpados.",
                    750, 280,
                    O(MissionObjectiveKind.Code, "Coordenar a correção", 3),
                    O(MissionObjectiveKind.Break, "Organizar a recuperação do time", 1),
                    O(MissionObjectiveKind.Meeting, "Conduzir a retrospectiva", 2))
            },

            // CTO — plataforma, escala e direção da empresa.
            new[]
            {
                new CareerMissionDefinition(
                    "PLATAFORMA AUTÔNOMA",
                    "A ByteForge precisa crescer sem multiplicar trabalho manual. Escale a plataforma, alinhe a transição e valide a entrega.",
                    "A plataforma passou a absorver o crescimento sem transformar cada deploy em evento corporativo.",
                    520, 320,
                    O(MissionObjectiveKind.ToolLevel, "Escalar a plataforma autônoma", 5),
                    O(MissionObjectiveKind.Meeting, "Alinhar a transição", 2),
                    O(MissionObjectiveKind.Code, "Validar a plataforma em produção", 3)),
                new CareerMissionDefinition(
                    "TECNOLOGIA COM CONTEXTO",
                    "O conselho quer uma resposta simples para um problema complexo. Estude os dados e apresente escolhas honestas.",
                    "O plano técnico ganhou orçamento porque explicou riscos, limites e impacto sem esconder as partes difíceis.",
                    1700, 350,
                    O(MissionObjectiveKind.Study, "Analisar os dados da empresa", 3),
                    O(MissionObjectiveKind.Meeting, "Apresentar opções ao conselho", 2),
                    O(MissionObjectiveKind.Code, "Preparar o plano de execução", 3)),
                new CareerMissionDefinition(
                    "PREPARAR O PRÓXIMO CAPÍTULO",
                    "A empresa não pode depender apenas de você. Fortaleça a liderança, entregue a direção técnica e prepare o futuro.",
                    "A ByteForge tem pessoas, processos e confiança para seguir adiante. Seu próximo cargo não cabe mais no organograma.",
                    700, 400,
                    O(MissionObjectiveKind.TeamSize, "Consolidar uma equipe com quatro colegas", 4),
                    O(MissionObjectiveKind.Meeting, "Compartilhar a direção técnica", 3),
                    O(MissionObjectiveKind.Study, "Registrar o plano de sucessão", 3))
            }
        };

        public static CareerMissionDefinition Get(CareerRank rank, int missionIndex)
        {
            int rankIndex = Math.Clamp((int)rank, 0, Missions.Length - 1);
            int index = Math.Clamp(missionIndex, 0, MissionsPerRank - 1);
            return Missions[rankIndex][index];
        }

        public static bool TryGetAction(MissionObjectiveKind kind, out OfficeActionType action)
        {
            switch (kind)
            {
                case MissionObjectiveKind.Code:
                    action = OfficeActionType.Code;
                    return true;
                case MissionObjectiveKind.Meeting:
                    action = OfficeActionType.Meeting;
                    return true;
                case MissionObjectiveKind.Study:
                    action = OfficeActionType.Study;
                    return true;
                case MissionObjectiveKind.Coffee:
                    action = OfficeActionType.Coffee;
                    return true;
                case MissionObjectiveKind.Break:
                    action = OfficeActionType.Break;
                    return true;
                default:
                    action = default;
                    return false;
            }
        }
    }
}
