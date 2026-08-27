# Imagens dos veículos

Um PNG por modelo, servido em `/vehicles/<slug>.png`. O nome sai do modelo
por `slugify()` em `pages/index.vue`: minúsculas, sem acentos, e tudo o que
não é letra ou número vira hífen.

| Modelo | Ficheiro |
|---|---|
| Seat Ateca | `seat-ateca.png` |
| Smart ForTwo | `smart-fortwo.png` |
| Yamaha Tenere | `yamaha-tenere.png` |
| Volkswagen Transporter | `volkswagen-transporter.png` |
| Mercedes-Benz Sprinter | `mercedes-benz-sprinter.png` |
| Mercedes-Benz Metris | `mercedes-benz-metris.png` |
| Mercedes-Benz Atego | `mercedes-benz-atego.png` |
| Volvo FL | `volvo-fl.png` |
| Volvo FH | `volvo-fh.png` |

Acrescentar um veículo não exige tocar em código: basta pousar aqui o
ficheiro com o nome certo. Enquanto ele faltar, o `VehiclePhoto` mostra um
marcador de posição em vez de uma imagem partida.

## Tamanho

**960 × 450 px**, PNG com fundo transparente, perfil virado para a direita.

O `VehiclePhoto` apresenta a imagem com no máximo **380 px** de largura, e a
altura acompanha. Os 960 px dão os 2× de que um ecrã de alta densidade
precisa, com alguma folga; acima disso só se paga transferência.

As três imagens já cá postas medem 975 × 434–450 px, portanto servem tal e
qual — a medida acima é para as próximas, não para refazer estas.

Mantém a proporção perto de **2,1:1** e o veículo com a mesma margem em
todas: como aparecem uma de cada vez ao trocar de veículo, uma imagem com
enquadramento diferente das outras dá um salto visível.

Convém ficarem **abaixo de ~150 KB**. As atuais estão entre 232 e 391 KB, o
que é bastante para uma imagem deste tamanho; passá-las por um compressor
de PNG corta a maior parte disso sem perda visível.
