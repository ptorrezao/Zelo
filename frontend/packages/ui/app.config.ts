// Identidade dos modulos: nao varia com o ambiente, ao contrario das urls,
// que estao no runtimeConfig. Cada app sobrepoe currentModule.
export default defineAppConfig({
  zelo: {
    currentModule: '',
    modules: [
      { key: 'shell', label: 'Início', icon: 'grid' },
      { key: 'auto', label: 'Auto', icon: 'truck' },
      { key: 'inventory', label: 'Inventário', icon: 'box' },
    ],
  },
})
