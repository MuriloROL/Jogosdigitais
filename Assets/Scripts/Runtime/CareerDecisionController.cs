using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class CareerDecisionController : MonoBehaviour
    {
        private readonly struct Choice
        {
            public Choice(string label, string result, int money, int knowledge, int reputation, int stress, int xp)
            {
                Label = label;
                Result = result;
                Money = money;
                Knowledge = knowledge;
                Reputation = reputation;
                Stress = stress;
                Xp = xp;
            }

            public string Label { get; }
            public string Result { get; }
            public int Money { get; }
            public int Knowledge { get; }
            public int Reputation { get; }
            public int Stress { get; }
            public int Xp { get; }
        }

        private readonly struct Decision
        {
            public Decision(string speaker, string title, string body, Choice first, Choice second)
            {
                Speaker = speaker;
                Title = title;
                Body = body;
                First = first;
                Second = second;
            }

            public string Speaker { get; }
            public string Title { get; }
            public string Body { get; }
            public Choice First { get; }
            public Choice Second { get; }
        }

        private static readonly Decision[] Decisions =
        {
            new("MARTA • SUPERVISORA", "SEU PRIMEIRO PROJETO REAL", "A API herdada está instável. Você prefere corrigir a base ou entregar rapidamente uma proteção temporária?",
                new Choice("REFATORAR A BASE", "Marta aprovou a refatoração: +12 conhecimento, +20 XP e mais tranquilidade.", 0, 12, 2, -4, 20),
                new Choice("CRIAR PROTEÇÃO RÁPIDA", "O cliente recebeu a correção a tempo: +10 reputação, +R$ 60 e pressão reduzida.", 60, 2, 10, -2, 5)),
            new("LIA • DEV PLENO", "A SPRINT ESTÁ APERTADA", "Uma funcionalidade importante ficou maior que o previsto. Como a equipe deve reagir?",
                new Choice("REDUZIR O ESCOPO", "A entrega ficou sustentável: -12 estresse, +6 reputação.", 0, 3, 6, -12, 10),
                new Choice("FAZER UM MUTIRÃO", "O mutirão voluntário antecipou a entrega e ganhou uma folga planejada: +35 XP.", 0, 2, 4, -4, 35)),
            new("MAYA • PRODUCT MANAGER", "FEEDBACK DO CLIENTE", "O cliente pediu uma mudança importante no fluxo. Você ouve mais usuários ou começa a implementar agora?",
                new Choice("PESQUISAR COM USUÁRIOS", "A pesquisa revelou o problema certo: +12 reputação, +8 conhecimento.", 0, 8, 12, -4, 15),
                new Choice("PROTOTIPAR AGORA", "O protótipo impressionou o cliente: +30 XP, +R$ 90 e uma direção mais clara.", 90, 3, 6, -2, 30)),
            new("THEO • TECH LEAD", "O TIME PRECISA ESCALAR", "Há orçamento para fortalecer a equipe ou acelerar o pipeline. Qual investimento vem primeiro?",
                new Choice("MENTORIA PARA O TIME", "A mentoria fortaleceu a cultura: +14 reputação, +10 conhecimento.", 0, 10, 14, -8, 10),
                new Choice("FERRAMENTAS DE ENTREGA", "O orçamento aprovado tornou o pipeline mais confiável: +40 XP, +6 conhecimento.", 0, 6, 4, -3, 40)),
            new("CLARA • CHIEF OF STAFF", "UMA DECISÃO DE CTO", "Uma parceria acelera o crescimento, mas exige acesso a dados internos. Como você responde?",
                new Choice("PROTEGER OS DADOS", "A ByteForge ganhou confiança: +18 reputação, +12 conhecimento.", 0, 12, 18, -6, 20),
                new Choice("NEGOCIAR A PARCERIA", "A parceria segura financiou a reta final: +R$ 220, +45 XP.", 220, 3, 5, -4, 45))
        };

        private GameObject overlay;
        private Text speakerLabel;
        private Text titleLabel;
        private Text bodyLabel;
        private Button firstButton;
        private Button secondButton;
        private Decision currentDecision;
        private CareerGame game;

        public bool IsVisible => overlay != null && overlay.activeSelf;

        private void Start()
        {
            BuildInterface();
            overlay.SetActive(false);
        }

        private void Update()
        {
            game ??= CareerGame.Current;
            if (game == null || IsVisible || !game.HasPendingCareerDecision)
            {
                return;
            }

            ShowDecision(game.PendingDecisionRank);
        }

        private void ShowDecision(CareerRank promotedRank)
        {
            int index = Mathf.Clamp((int)promotedRank - 1, 0, Decisions.Length - 1);
            currentDecision = Decisions[index];
            speakerLabel.text = currentDecision.Speaker;
            titleLabel.text = currentDecision.Title;
            bodyLabel.text = currentDecision.Body;
            firstButton.GetComponentInChildren<Text>().text = currentDecision.First.Label;
            secondButton.GetComponentInChildren<Text>().text = currentDecision.Second.Label;
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
        }

        private void ChooseFirst()
        {
            Resolve(currentDecision.First);
        }

        private void ChooseSecond()
        {
            Resolve(currentDecision.Second);
        }

        private void Resolve(Choice choice)
        {
            game?.ResolveCareerDecision(choice.Result, choice.Money, choice.Knowledge, choice.Reputation, choice.Stress, choice.Xp);
            overlay.SetActive(false);
            GameAudioFeedback.Play(FeedbackSound.Decision);
        }

        private void BuildInterface()
        {
            overlay = CreateImage(transform, "Decisão de carreira", new Color(0.006f, 0.012f, 0.024f, 0.9f)).gameObject;
            SetRect((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.GetComponent<Image>().raycastTarget = true;

            Image panel = CreateImage(overlay.transform, "Conversa com a equipe", new Color(0.04f, 0.065f, 0.105f, 0.995f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 450f));
            panel.color = Color.white;
            panel.gameObject.AddComponent<UiRoundedGraphic>().Configure(20);
            panel.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.10f, 0.13f, 0.19f, 0.995f),
                new Color(0.035f, 0.055f, 0.095f, 0.995f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.63f, 0.20f, 0.50f);
            outline.effectDistance = new Vector2(2f, -2f);

            Image accent = CreateImage(panel.transform, "Linha da decisão", new Color(0.96f, 0.63f, 0.20f));
            SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(16f, -2f), new Vector2(-32f, 4f), new Vector2(0.5f, 1f));
            accent.gameObject.AddComponent<UiRoundedGraphic>().Configure(4);

            speakerLabel = CreateText(panel.transform, "COLEGA", 12, TextAnchor.MiddleLeft, new Color(0.24f, 0.82f, 0.76f), FontStyle.Bold);
            SetRect(speakerLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.up * -40f, new Vector2(-76f, 24f));
            titleLabel = CreateText(panel.transform, "DECISÃO", 28, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(titleLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.up * -84f, new Vector2(-76f, 46f));
            bodyLabel = CreateText(panel.transform, "Escolha como responder.", 18, TextAnchor.UpperLeft, new Color(0.80f, 0.87f, 0.95f));
            bodyLabel.lineSpacing = 1.28f;
            SetRect(bodyLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.up * -190f, new Vector2(-76f, 130f));

            Text hint = CreateText(panel.transform, "TODAS AS OPÇÕES SÃO POSITIVAS • ESCOLHA O BENEFÍCIO QUE PREFERE", 10, TextAnchor.MiddleCenter, new Color(0.62f, 0.70f, 0.82f), FontStyle.Bold);
            SetRect(hint.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.up * 161f, new Vector2(-76f, 20f));

            firstButton = CreateButton(panel.transform, "OPÇÃO A", new Vector2(38f, 70f), new Vector2(326f, 72f), Vector2.zero, new Color(0.19f, 0.48f, 0.47f));
            firstButton.onClick.AddListener(ChooseFirst);
            secondButton = CreateButton(panel.transform, "OPÇÃO B", new Vector2(-38f, 70f), new Vector2(326f, 72f), new Vector2(1f, 0f), new Color(0.48f, 0.31f, 0.16f));
            secondButton.onClick.AddListener(ChooseSecond);
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var instance = new GameObject(name, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
            Image image = instance.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(Transform parent, string value, int size, TextAnchor anchor, Color color, FontStyle style = FontStyle.Normal)
        {
            var instance = new GameObject(value, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
            Text text = instance.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor, Color color)
        {
            Image image = CreateImage(parent, label, color);
            SetRect(image.rectTransform, anchor, anchor, position, size, anchor);
            image.raycastTarget = true;
            image.color = Color.white;
            image.gameObject.AddComponent<UiRoundedGraphic>().Configure(12);
            image.gameObject.AddComponent<UiVerticalGradient>().Configure(
                Color.Lerp(color, Color.white, 0.08f),
                Color.Lerp(color, Color.black, 0.22f));
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            button.colors = colors;
            image.gameObject.AddComponent<UiButtonMotion>();
            Text text = CreateText(image.transform, label, 14, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 2f), new Vector2(-20f, -4f));
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
