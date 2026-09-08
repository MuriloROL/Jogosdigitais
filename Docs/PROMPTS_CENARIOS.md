# Prompts de referência para criação dos cenários

**Modelo informado pelo responsável pelo projeto:** GPT 6 Astra, da OpenAI.

Este documento reúne prompts de referência que descrevem a direção artística
dos quatro cenários de fundo do projeto.

## Objetivo das imagens

Criar quatro fundos de escritório para o **Dev Career Idle Game**, representando
a evolução de Alex na empresa ByteForge: estágio, desenvolvimento, liderança e
diretoria de tecnologia. Os arquivos existentes são PNG de 1536 × 1024 pixels.

## Prompt base de referência

> Crie uma imagem de cenário de fundo para um jogo 2D de carreira de
> desenvolvimento de software chamado Dev Career Idle Game, ambientado na
> empresa fictícia ByteForge. Represente um escritório de tecnologia visto de
> cima, com uma leve inclinação que permita enxergar a frente dos móveis.
> Use estética de pixel art detalhada, com formas bem definidas e composição
> apropriada para um jogo de computador.
>
> Mostre o escritório inteiro como um ambiente sem teto, com paredes delimitando
> o espaço, piso de placas regulares e corredores amplos. Reserve uma área livre
> no centro para a circulação do personagem. Distribua cinco áreas de atividade:
> programação na parte superior esquerda, reunião na parte superior direita,
> estudo na parte inferior esquerda, café na parte inferior central e descanso
> na parte inferior direita.
>
> Use azul-marinho, cinza e madeira como base. Acrescente luzes ciano nos
> monitores e detalhes tecnológicos, iluminação quente em âmbar nas luminárias
> e verde nas plantas. Inclua computadores, cadeiras, livros, armários e objetos
> de escritório que tornem o ambiente habitado e reconhecível.
>
> Entregue uma única ilustração horizontal em PNG, na proporção 3:2, com
> resolução de 1536 × 1024 pixels. Mantenha o ambiente inteiro dentro do quadro.
> Não inclua personagens, interface, botões, marcadores de missão, logotipos,
> marcas-d'água ou textos legíveis. O fundo será importado no Unity, e os
> personagens e elementos interativos serão adicionados separadamente.

## Variações por cenário

Cada trecho abaixo complementa o prompt base e descreve a direção artística
para uma das quatro imagens.

### Estagiário — `office-intern.png`

> Crie um escritório pequeno e acolhedor para o começo da carreira. Inclua uma
> mesa compartilhada com computadores à esquerda e o espaço da supervisora à
> direita. Na parte inferior, coloque uma estante com manuais, uma pequena copa
> com cafeteira e um sofá simples. Use mobiliário funcional, poucos recursos e
> detalhes que transmitam aprendizado e rotina de uma equipe pequena. Preserve
> espaço livre entre as estações.

### Desenvolvedor — `office-developer.png`

> Crie um escritório maior para uma equipe de desenvolvedores júnior e pleno.
> Inclua mais estações com computadores e monitores, uma área de reunião,
> biblioteca técnica, copa e lounge. Mostre um ambiente organizado e colaborativo,
> com mais equipamentos e conforto que o escritório de estágio. Mantenha a
> disposição geral das cinco atividades e corredores largos entre elas.

### Liderança — `office-leadership.png`

> Crie um escritório para desenvolvedores sêniores e líderes técnicos. Destaque
> uma estação de arquitetura de software e uma sala de reunião para planejamento
> e acompanhamento de projetos, com telas e diagramas abstratos. Inclua uma área
> de conhecimento e mentoria, café e um lounge confortável. Transmita maior
> responsabilidade e capacidade técnica por meio de equipamentos, organização
> e acabamento mais sofisticados, mantendo a identidade visual dos outros fundos.

### CTO — `office-cto.png`

> Crie um escritório executivo de tecnologia com acabamento sofisticado. No alto
> à esquerda, coloque uma grande mesa de madeira com vários monitores e uma
> janela com vista para a cidade à noite. No alto à direita, inclua uma sala de
> reunião envidraçada com mesa ampla e painel tecnológico. Acrescente racks de
> servidores na lateral direita, estantes com livros e troféus à esquerda,
> poltronas de leitura, uma copa central e um lounge com sofás azul-escuros.
> Use luminárias âmbar, telas ciano e plantas, mantendo o centro livre para
> circulação e a mesma perspectiva dos escritórios anteriores.

## Arquivos e uso atual no Unity

As quatro imagens ficam em [`Assets/Art/GeneratedOffices/`](../Assets/Art/GeneratedOffices/).
O importador `GeneratedOfficeArtImporter` configura cada PNG como um sprite
único, com filtro Point, sem mipmaps e sem compressão de textura.

O componente `OfficeEnvironment` exibe e dimensiona a imagem de fundo. O gerador
`OfficeMvpSceneBuilder` posiciona personagens, áreas de interação e interface
nas cenas. Os móveis desenhados no fundo fazem parte da imagem e não são objetos
individuais do cenário. Os personagens usados no jogo vêm do pacote Free TopDown
RPG Retro Sprites! / Fantasy Dreamland, da ELV Games.
