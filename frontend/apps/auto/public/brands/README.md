# Logótipos das marcas

Um PNG por marca, servido em `/brands/<slug>.png`. O nome sai do campo
`brand` do veículo pela mesma regra das fotografias: minúsculas, sem
acentos, e o resto convertido em hífen.

| Marca | Ficheiro |
|---|---|
| Yamaha | `yamaha.png` |
| Seat | `seat.png` |
| Smart | `smart.png` |
| Mercedes-Benz | `mercedes-benz.png` |
| Volkswagen | `volkswagen.png` |
| Volvo | `volvo.png` |

Enquanto o ficheiro faltar, o avatar mostra as iniciais da marca e do
modelo, e a consola regista um 404 por marca em falta.

## Tamanho

**128 × 128 px**, quadrado, PNG com fundo transparente.

O avatar da lista tem 36 px e o do cabeçalho 44 px, aos quais o componente
tira 15% de recuo. Os 128 px cobrem 3× no maior dos dois casos, que é o
suficiente para um ecrã de alta densidade — um logótipo não ganha nada em
ser maior do que isto.

Quadrado é indispensável: o avatar é redondo e uma imagem alongada fica
descentrada. O logótipo deve vir com margem própria mínima, porque o recuo
do avatar já lhe dá ar; margem a dobrar deixa a marca minúscula no círculo.

Poucos KB cada um. Se a marca tiver versão vetorial, exporta daí em vez de
ampliar um ficheiro pequeno.
