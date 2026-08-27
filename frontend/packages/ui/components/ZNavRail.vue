<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const { zelo } = useAppConfig()
const urls = useRuntimeConfig().public.zelo as Record<string, string>

// TODO: Get user from auth store
const user = ref<{ email: string } | null>({ email: 'user@example.com' })

async function handleLogout() {
  // Clear auth token
  const token = useCookie('auth_token')
  token.value = null

  // Redirect to shell login page (base URL)
  window.location.href = '/login'
}

// Cookie em vez de localStorage: o servidor le-o e ja rende o rail no
// estado certo, sem o salto que a leitura no cliente provocaria. Com
// path na raiz, a preferencia acompanha o utilizador de modulo para
// modulo, que aqui e sempre um carregamento de pagina novo.
const pinned = useCookie<boolean>('zelo-rail-pinned', {
  default: () => false,
  path: '/',
  sameSite: 'lax',
  maxAge: 60 * 60 * 24 * 365,
})

const panelId = useId()

// Um modulo novo entra pelo app.config e escolhe aqui o seu desenho.
const icons: Record<string, string> = {
  grid: 'M3 3h6v6H3zM11 3h6v6h-6zM3 11h6v6H3zM11 11h6v6h-6z',
  truck: 'M2 5h9v8H2zM11 8h4l3 3v2h-7zM5.5 15.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3zM14.5 15.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z',
  box: 'M10 2 3 5.5v9L10 18l7-3.5v-9zM3 5.5 10 9l7-3.5M10 9v9',
}
</script>

<template>
  <!-- Fechado, o <nav> reserva so a largura dos icones e o painel cresce
       por cima ao passar o rato, para nada saltar de sitio. Fixo, passa a
       ocupar a largura aberta: se ficasse por cima em permanencia, tapava
       conteudo para sempre. -->
  <nav
    class="z-rail"
    :class="{ 'z-rail--pinned': pinned }"
    aria-label="Módulos"
  >
    <div :id="panelId" class="z-rail__panel">
      <!-- A linha inteira e o botao: da um alvo folgado para o dedo, que e
           o unico caminho onde nao ha rato. -->
      <div class="z-rail__head">
        <button
          type="button"
          class="z-rail__toggle"
          :aria-expanded="pinned"
          :aria-controls="panelId"
          :aria-label="pinned ? 'Zelo — fechar menu' : 'Zelo — abrir menu'"
          @click="pinned = !pinned"
        >
          <svg class="z-rail__icon" viewBox="0 0 20 20" aria-hidden="true">
            <path
              d="M7 4l5 6-5 6"
              fill="none"
              stroke="currentColor"
              stroke-width="1.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
          <span class="z-rail__name">Zelo</span>
        </button>
      </div>

      <!-- O invólucro existe para a versão de telemóvel poder animar a
           abertura para baixo, que não se consegue com height: auto. -->
      <div class="z-rail__body">
        <div class="z-rail__clip">
          <ul class="z-rail__list">
            <li v-for="mod in zelo.modules" :key="mod.key">
              <a
                class="z-rail__link"
                :class="{ 'z-rail__link--current': mod.key === zelo.currentModule }"
                :href="urls[mod.key]"
                :aria-current="mod.key === zelo.currentModule ? 'page' : undefined"
              >
                <svg class="z-rail__icon" viewBox="0 0 20 20" aria-hidden="true">
                  <path
                    :d="icons[mod.icon] ?? icons.grid"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.5"
                    stroke-linejoin="round"
                  />
                </svg>
                <span class="z-rail__name">{{ mod.label }}</span>
              </a>
            </li>

            <li class="z-rail__divider" />

            <li>
              <a
                :href="`${urls.shell}/profile`"
                class="z-rail__link"
                :title="`Profile: ${user?.email}`"
              >
                <svg class="z-rail__icon" viewBox="0 0 20 20" aria-hidden="true">
                  <path
                    d="M10 10a2 2 0 100-4 2 2 0 000 4zM1 18a8 8 0 1116 0H1z"
                    fill="currentColor"
                  />
                </svg>
                <span class="z-rail__name">{{ user?.email || 'User' }}</span>
              </a>
            </li>

            <li>
              <a
                href="#"
                class="z-rail__link"
                @click.prevent="handleLogout"
                title="Logout"
              >
                <svg class="z-rail__icon" viewBox="0 0 20 20" aria-hidden="true">
                  <path
                    d="M17 6l-5.293 5.293a1 1 0 101.414 1.414L18.414 7.414a2 2 0 000-2.828l-1.414-1.414a1 1 0 00-1.414 1.414L17 6zM3 5a2 2 0 00-2 2v6a2 2 0 002 2h5v-2H3V7h5V5H3z"
                    fill="currentColor"
                  />
                </svg>
                <span class="z-rail__name">Logout</span>
              </a>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.z-rail {
  --z-rail-closed: 60px;
  --z-rail-open: 190px;

  position: relative;
  width: var(--z-rail-closed);
  transition: width 180ms ease;
  /* Sai para fora do respiro do layout para encostar a esquerda, ao topo e
     ao fundo. O valor e o simetrico do padding do .z-shell; o espaco que
     separa do conteudo continua a ser o gap. */
  margin: calc(var(--z-space-3) * -1) 0 calc(var(--z-space-3) * -1) calc(var(--z-space-3) * -1);
}

.z-rail--pinned {
  width: var(--z-rail-open);
}

.z-rail__panel {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  z-index: 20;
  width: var(--z-rail-closed);
  padding: var(--z-space-4) 0;
  box-sizing: border-box;
  background: var(--z-color-rail);
  /* So o lado que nao esta encostado leva canto redondo. */
  border-radius: 0 var(--z-radius-lg) var(--z-radius-lg) 0;
  /* O texto fica no documento mesmo fechado — sai apenas do recorte —
     para continuar disponivel a leitores de ecra. */
  overflow: hidden;
  transition: width 180ms ease;
  display: flex;
  flex-direction: column;
}

/* O teclado abre o painel em qualquer aparelho: sem isto, quem navega
   com Tab nunca veria os nomes. */
.z-rail:focus-within .z-rail__panel {
  width: var(--z-rail-open);
}

/* O rato so comanda onde existe mesmo um. Em ecra tatil o toque no botao
   e a unica via, e e por isso que o botao esta sempre visivel. */
@media (hover: hover) and (pointer: fine) {
  .z-rail:hover .z-rail__panel {
    width: var(--z-rail-open);
    /* Atraso de intencao: abre so se o rato ficar: atravessar a margem a
       caminho de outra coisa nao chega. A fechar nao ha atraso nenhum. */
    transition-delay: 150ms;
  }
}

.z-rail--pinned .z-rail__panel {
  width: var(--z-rail-open);
  transition-delay: 0ms;
}

.z-rail__head {
  margin-bottom: var(--z-space-4);
  /* mesmo recuo da lista, para o icone do botao alinhar com os dos modulos */
  padding: 0 10px;
}

.z-rail__body {
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.z-rail__divider {
  height: 1px;
  margin: var(--z-space-2) 0;
  background: rgb(255 255 255 / 8%);
}

.z-rail__toggle {
  display: flex;
  align-items: center;
  gap: var(--z-space-3);
  width: 100%;
  height: 40px;
  padding: 0 10px;
  box-sizing: border-box;
  border: 0;
  border-radius: var(--z-radius);
  background: none;
  font: inherit;
  font-weight: 600;
  color: var(--z-color-rail-text-active);
  text-align: left;
  cursor: pointer;
}

.z-rail__toggle:hover {
  background: rgb(255 255 255 / 8%);
}

.z-rail__toggle .z-rail__icon {
  transition: transform 180ms ease;
}

.z-rail--pinned .z-rail__toggle .z-rail__icon {
  transform: rotate(180deg);
}

.z-rail__list {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-2);
  margin: 0;
  padding: 0 10px;
  list-style: none;
}

.z-rail__link {
  display: flex;
  align-items: center;
  gap: var(--z-space-3);
  height: 40px;
  padding: 0 10px;
  box-sizing: border-box;
  border-radius: var(--z-radius);
  color: var(--z-color-rail-text);
  text-decoration: none;
}

.z-rail__icon {
  width: 20px;
  height: 20px;
  flex: none;
}

.z-rail__name {
  font-size: var(--z-font-size-sm);
  white-space: nowrap;
  opacity: 0;
  transition: opacity 140ms ease;
}

.z-rail:focus-within .z-rail__name,
.z-rail--pinned .z-rail__name {
  opacity: 1;
}

@media (hover: hover) and (pointer: fine) {
  .z-rail:hover .z-rail__name {
    opacity: 1;
    transition-delay: 150ms;
  }
}

.z-rail__link:hover {
  background: rgb(255 255 255 / 8%);
  color: var(--z-color-rail-text-active);
}

.z-rail__link--current {
  background: var(--z-color-accent);
  color: var(--z-color-accent-text);
}

.z-rail__link--current:hover {
  background: var(--z-color-accent);
}

/* Em ecra estreito o rail deixa de ser uma coluna e passa a barra no topo,
   que abre para baixo por cima do conteudo. Vem depois das regras de rato
   de proposito: num portatil tatil e estreito as duas condicoes aplicam-se
   e e esta que deve mandar. */
@media (max-width: 900px) {
  .z-rail {
    width: auto;
    /* reserva so a barra fechada; o resto abre por cima */
    height: 72px;
    /* encosta a esquerda, a direita e ao topo; o fundo fica solto */
    margin: calc(var(--z-space-3) * -1) calc(var(--z-space-3) * -1) 0;
  }

  /* Todos os estados listados: as media queries nao acrescentam
     especificidade, portanto sem isto a regra de "fixo" continuava a impor
     os 190px da coluna. */
  .z-rail__panel,
  .z-rail:hover .z-rail__panel,
  .z-rail:focus-within .z-rail__panel,
  .z-rail--pinned .z-rail__panel {
    right: 0;
    bottom: auto;
    width: auto;
    border-radius: 0 0 var(--z-radius-lg) var(--z-radius-lg);
    transition: none;
  }

  /* 0fr -> 1fr anima a abertura, coisa que height: auto nao faz. */
  .z-rail__body {
    display: grid;
    grid-template-rows: 0fr;
    transition: grid-template-rows 180ms ease;
  }

  .z-rail--pinned .z-rail__body {
    grid-template-rows: 1fr;
  }

  /* O afastamento passa do cabecalho para dentro da lista: fora dela
     contaria mesmo com o menu fechado, e a barra tapava conteudo. */
  .z-rail__head {
    margin-bottom: 0;
  }

  /* O recorte tem de estar um nivel acima da lista: o padding dela nao e
     cortado pelo seu proprio overflow e ficavam 16px de barra a mais. */
  .z-rail__clip {
    overflow: hidden;
  }

  .z-rail__list {
    padding-top: var(--z-space-4);
    /* fechada, a lista sai da ordem de tabulacao em vez de ficar
       invisivel mas ainda alcancavel pelo teclado */
    visibility: hidden;
    transition: visibility 0s linear 180ms;
  }

  .z-rail--pinned .z-rail__list {
    visibility: visible;
    transition-delay: 0s;
  }

  /* Ha largura de sobra: os nomes nao dependem de nada para aparecer. */
  .z-rail__name,
  .z-rail:hover .z-rail__name {
    opacity: 1;
    transition-delay: 0s;
  }

  /* Cara de barra de aplicacao: nome a esquerda, seta a direita. */
  .z-rail__toggle {
    flex-direction: row-reverse;
    justify-content: space-between;
  }

  .z-rail__toggle .z-rail__icon {
    transform: rotate(90deg);
  }

  .z-rail--pinned .z-rail__toggle .z-rail__icon {
    transform: rotate(-90deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .z-rail,
  .z-rail__panel,
  .z-rail__body,
  .z-rail__name,
  .z-rail__icon {
    transition: none;
  }
}
</style>
