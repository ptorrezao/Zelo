// Catalogo estatico de marcas/modelos para os dropdowns do formulario de
// veiculo. TODO: substituir por uma chamada a API quando existir.
export const VEHICLE_CATALOG: Record<string, string[]> = {
  Yamaha: ['Tenere', 'MT-07', 'MT-09', 'XSR700'],
  Honda: ['Civic', 'CR-V', 'CB500F', 'PCX125'],
  Seat: ['Ateca', 'Leon', 'Ibiza', 'Arona'],
  Smart: ['ForTwo', 'ForFour'],
  Volkswagen: ['Golf', 'Polo', 'T-Roc', 'Tiguan'],
  BMW: ['Serie 1', 'Serie 3', 'X1', 'R1250GS'],
  Toyota: ['Corolla', 'Yaris', 'RAV4'],
  Renault: ['Clio', 'Captur', 'Megane'],
}

export const VEHICLE_BRANDS = Object.keys(VEHICLE_CATALOG)
