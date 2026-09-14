// Endpoint de descoberta: dado o URL do site (o que o utilizador visita
// no browser), diz qual e o URL da API por trás dele. Existe para a
// importação de veículos entre ambientes (AUTO) - o utilizador só
// conhece/cola o URL do site, nunca o da API, que é um detalhe de
// deployment que não lhe compete saber.
//
// apiBase já é "public" runtime config (vai para o browser em todas as
// páginas de qualquer forma) - expor aqui não adiciona nenhum segredo novo.
export default defineEventHandler((event) => {
  const config = useRuntimeConfig(event)
  return { apiBase: config.public.apiBase }
})
