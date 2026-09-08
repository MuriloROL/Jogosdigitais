# Personagens — ELV Games

- Pack: **Free TopDown RPG Retro Sprites! / Free Character Sprites — Fantasy Dreamland**.
- Autor: **ELV Games**.
- Fonte: https://elvgames.itch.io/free-retro-game-world-sprites
- Download: versão gratuita, arquivo `Free Character Sprites - Fantasy Dreamland.zip`.
- SHA-256 do ZIP: `0a273e8489615c03b8aeb816d66de60740c124ef878cae44dc8403a35ea48d78`.
- Licença original: [License.txt](License.txt), preservada sem alteração.

Os 12 PNGs foram copiados sem edição da pasta `16x16` do download. Apesar do
nome da pasta, cada arquivo tem 96 × 96 pixels e contém uma grade 4 × 4 de
quadros de 24 × 24 pixels. Não foram importadas as versões ampliadas ou os
arquivos específicos de RPG Maker.

## Integração

O Unity recorta as folhas em quatro quadros por direção: baixo, esquerda,
direita e cima, nessa ordem de cima para baixo. Os arquivos `_Idle` contêm a
animação parado; os demais contêm caminhada.

A importação usa filtro Point, sem mipmaps ou compressão, e 12 pixels por
unidade. O pivô mantém o centro dos pés em x=0,5 no sistema de coordenadas
existente do personagem. A folha de caminhada recebe compensação vertical de
um pixel para alinhar a sola ao quadro parado. Essas configurações não alteram
os PNGs originais.

| Personagem no jogo | Sprite |
|---|---|
| Alex, em todos os cargos e no final | `Char_003` |
| Marta e Clara | `Char_001` |
| Caio | `Char_002` |
| Ravi e Theo | `Char_004` |
| Lia e Nina | `Char_005` |
| Maya | `Char_006` |

Os créditos também aparecem no menu principal do jogo. Os fundos do escritório
e a arte da interface são assets separados deste pacote.

O inventário e as orientações de manutenção estão em
[Assets de personagens](../../../Docs/ASSETS_PERSONAGENS.md).
