# Visão do jogo

Um simulador de vida 2D inspirado na fantasia central de *The Sims*: cuidar de personagens, administrar necessidades, organizar a casa e encadear interações em um mundo cotidiano vivo.

## Primeiro protótipo jogável

- Uma casa pequena construída em grade 16×16.
- Um personagem selecionável com movimentação por clique.
- Quatro necessidades: fome, energia, higiene e social.
- Objetos interativos que satisfazem necessidades.
- Relógio do jogo com pausa e velocidades diferentes.
- Fila visível de ações, permitindo cancelar e reordenar tarefas.
- Salvamento local de personagem, necessidades, tempo e objetos da casa.

## Sistemas principais

1. `Sim`: identidade, atributos, necessidades e estado atual.
2. `Interaction`: ações anunciadas pelos objetos, com custo, duração e efeitos.
3. `ActionQueue`: planejamento e execução sequencial das ações do personagem.
4. `WorldGrid`: ocupação, posicionamento de móveis e navegação dentro da casa.
5. `GameClock`: passagem de tempo, pausa e velocidades de simulação.
6. `BuildMode`: catálogo, compra, rotação e venda de objetos.
7. `SaveGame`: estado persistente separado dos dados de configuração.

## Direção visual inicial

- Pixel art nativa em 16×16 pixels por unidade.
- Câmera ortográfica com filtro Point e sem mipmaps.
- Modern Interiors como base para cômodos, móveis, personagens e UI.
- Crédito obrigatório: `limezu.itch.io`.

## Limite de escopo

O primeiro marco é uma única casa com um personagem e um ciclo completo de necessidades. Bairro, carreiras, relacionamentos profundos, construção de múltiplos andares e criação modular de personagens ficam para marcos posteriores.
