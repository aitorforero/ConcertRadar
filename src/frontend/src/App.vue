<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'

type Concert = {
  id: string
  name: string
  description?: string | null
  startDate: string
  endDate?: string | null
  venue: string
  city?: string | null
  address?: string | null
  ticketUrl?: string | null
  artists: Artist[]
}

type User = {
  id: string
  name: string
  email: string
}

type Artist = {
  id: string
  name: string
  isFollowed: boolean
}

const concerts = ref<Concert[]>([])
const search = ref('')
const selectedCity = ref('Todas')
const fromDate = ref('')
const toDate = ref('')
const isLoading = ref(true)
const isOffline = ref(false)
const activeView = ref('Descubrir')
const currentUser = ref<User | null>(null)
const authPanelOpen = ref(false)
const userMenuOpen = ref(false)
const authMode = ref<'login' | 'register'>('login')
const authError = ref('')
const authLoading = ref(false)
const authForm = ref({ name: '', email: '', password: '' })
const profileForm = ref({ name: '' })
const passwordForm = ref({ currentPassword: '', newPassword: '' })
const accountMessage = ref('')
const accountError = ref('')
const artistMenuConcertId = ref<string | null>(null)
const actionError = ref('')

const cities = computed(() => ['Todas', ...new Set(concerts.value.map((concert) => concert.city).filter(Boolean) as string[])])
const filteredConcerts = computed(() => [...concerts.value].sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime()))
const nextConcert = computed(() => filteredConcerts.value[0])

function formatDate(date: string) {
  return new Intl.DateTimeFormat('es-ES', { weekday: 'short', day: '2-digit', month: 'short' }).format(new Date(date)).replace('.', '')
}

function formatTime(date: string) {
  return new Intl.DateTimeFormat('es-ES', { hour: '2-digit', minute: '2-digit' }).format(new Date(date))
}

function dayNumber(date: string) {
  return new Intl.DateTimeFormat('es-ES', { day: '2-digit' }).format(new Date(date))
}

function monthName(date: string) {
  return new Intl.DateTimeFormat('es-ES', { month: 'short' }).format(new Date(date)).replace('.', '')
}

async function loadConcerts() {
  isLoading.value = true
  try {
    const params = new URLSearchParams()
    const trimmedSearch = search.value.trim()
    const trimmedCity = selectedCity.value.trim()

    if (trimmedSearch) {
      params.set('search', trimmedSearch)
    }

    if (trimmedCity && trimmedCity !== 'Todas') {
      params.set('city', trimmedCity)
    }

    if (fromDate.value) {
      params.set('fromDate', fromDate.value)
    }

    if (toDate.value) {
      params.set('toDate', toDate.value)
    }

    const query = params.toString() ? `?${params.toString()}` : ''
    const response = await fetch(`/api/events${query}`)

    if (!response.ok) {
      throw new Error('API unavailable')
    }

    const data = await response.json()
    concerts.value = Array.isArray(data) ? data : []
    isOffline.value = false
  } catch {
    concerts.value = []
    isOffline.value = true
  } finally {
    isLoading.value = false
  }
}

async function loadCurrentUser() {
  try {
    const response = await fetch('/api/auth/me', { credentials: 'include' })
    if (response.ok) {
      currentUser.value = await response.json()
      profileForm.value.name = currentUser.value?.name ?? ''
    }
  } catch {
    currentUser.value = null
  }
}

function openAuth(mode: 'login' | 'register') {
  authMode.value = mode
  authError.value = ''
  authForm.value = { name: '', email: '', password: '' }
  authPanelOpen.value = true
}

function openAccountMenu() {
  userMenuOpen.value = !userMenuOpen.value
}

function openProfile() {
  userMenuOpen.value = false
  accountMessage.value = ''
  accountError.value = ''
  authPanelOpen.value = true
}

async function submitAuth() {
  authLoading.value = true
  authError.value = ''
  try {
    const response = await fetch(`/api/auth/${authMode.value}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(authForm.value),
    })
    const data = await response.json().catch(() => null)
    if (!response.ok) {
      authError.value = typeof data === 'string' ? data : 'No se ha podido completar la operación.'
      return
    }
    currentUser.value = data
    profileForm.value.name = data.name
    authPanelOpen.value = false
    await loadConcerts()
  } catch {
    authError.value = 'La API no está disponible.'
  } finally {
    authLoading.value = false
  }
}

async function updateProfile() {
  accountMessage.value = ''
  accountError.value = ''
  const response = await fetch('/api/auth/me', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({ name: profileForm.value.name, email: currentUser.value?.email }),
  })
  if (!response.ok) {
    accountError.value = 'No se ha podido actualizar el nombre.'
    return
  }
  currentUser.value = await response.json()
  accountMessage.value = 'Datos actualizados.'
}

async function changePassword() {
  accountMessage.value = ''
  accountError.value = ''
  const response = await fetch('/api/auth/me/password', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(passwordForm.value),
  })
  if (!response.ok) {
    const data = await response.json().catch(() => null)
    accountError.value = typeof data === 'string' ? data : 'No se ha podido cambiar la contraseña.'
    return
  }
  passwordForm.value = { currentPassword: '', newPassword: '' }
  accountMessage.value = 'Contraseña actualizada.'
}

async function deleteAccount() {
  const password = window.prompt('Introduce tu contraseña para eliminar la cuenta.')
  if (password === null) return

  const response = await fetch('/api/auth/me', {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({ password }),
  })
  if (!response.ok) {
    accountError.value = 'No se ha podido eliminar la cuenta.'
    return
  }
  currentUser.value = null
  userMenuOpen.value = false
  authPanelOpen.value = false
}

async function logout() {
  await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
  currentUser.value = null
  userMenuOpen.value = false
  authPanelOpen.value = false
  await loadConcerts()
}

function toggleArtistMenu(concertId: string) {
  artistMenuConcertId.value = artistMenuConcertId.value === concertId ? null : concertId
  actionError.value = ''
}

async function updateArtistFollow(artist: Artist, shouldFollow: boolean) {
  if (!currentUser.value) {
    artistMenuConcertId.value = null
    openAuth('login')
    return
  }

  const response = await fetch(`/api/bands/${artist.id}/follow`, {
    method: shouldFollow ? 'POST' : 'DELETE',
    credentials: 'include',
  })
  if (!response.ok) {
    actionError.value = shouldFollow ? 'No se ha podido seguir al artista.' : 'No se ha podido dejar de seguir al artista.'
    return
  }

  concerts.value = concerts.value.map((concert) => ({
    ...concert,
    artists: concert.artists.map((candidate) => candidate.id === artist.id
      ? { ...candidate, isFollowed: shouldFollow }
      : candidate),
  }))
  artistMenuConcertId.value = null
}

watch(search, () => {
  void loadConcerts()
})

watch(selectedCity, () => {
  void loadConcerts()
})

watch([fromDate, toDate], () => {
  void loadConcerts()
})

onMounted(async () => {
  await loadCurrentUser()
  await loadConcerts()
})
</script>

<template>
  <div class="app-shell">
    <header class="topbar">
      <a class="brand" href="#" aria-label="ConcertRadar, inicio">
        <span class="brand-mark"><span></span><span></span><span></span></span>
        <span>Concert<span>Radar</span></span>
      </a>
      <nav class="main-nav" aria-label="Navegación principal">
        <button :class="{ active: activeView === 'Descubrir' }" @click="activeView = 'Descubrir'">Descubrir</button>
        <button :class="{ active: activeView === 'Mi agenda' }" @click="activeView = 'Mi agenda'">Mi agenda <small>0</small></button>
      </nav>
      <div v-if="currentUser" class="user-menu-wrapper">
        <button class="profile-button" aria-label="Abrir menú de usuario" :aria-expanded="userMenuOpen" @click="openAccountMenu">{{ currentUser.name.slice(0, 2).toUpperCase() }}</button>
        <div v-if="userMenuOpen" class="user-dropdown" aria-label="Opciones de usuario">
          <button class="account-menu-item" @click="openProfile">Mis datos <span>→</span></button>
          <button class="account-menu-item logout-item" @click="logout">Cerrar sesión <span>↗</span></button>
        </div>
      </div>
      <button v-else class="profile-button profile-login" aria-label="Iniciar sesión" @click="openAuth('login')">Entrar</button>
    </header>

    <main>
      <section class="intro-section">
        <div class="eyebrow"><span class="pulse-dot"></span> Radar musical <span class="slash">/</span> España</div>
        <h1>Tu próxima<br /><em>noche inolvidable.</em></h1>
        <p class="intro-copy">Encuentra conciertos que merecen salir<br class="desktop-only" /> de tu calendario.</p>
        <div class="search-row">
          <label class="search-box">
            <span aria-hidden="true">⌕</span>
            <input v-model="search" type="search" placeholder="Artista, sala o ciudad..." />
            <kbd>⌘ K</kbd>
          </label>
          <div class="city-filter">
            <span aria-hidden="true">⌖</span>
            <select v-model="selectedCity" aria-label="Filtrar por ciudad">
              <option v-for="city in cities" :key="city">{{ city }}</option>
            </select>
          </div>
          <label class="date-filter">
            <span>Desde</span>
            <input v-model="fromDate" type="date" aria-label="Filtrar desde una fecha" />
          </label>
          <label class="date-filter">
            <span>Hasta</span>
            <input v-model="toDate" type="date" aria-label="Filtrar hasta una fecha" />
          </label>
        </div>
      </section>

      <section class="content-section">
        <div class="section-heading">
          <div><span class="section-kicker">AGENDA ABIERTA</span><h2>Próximos conciertos</h2></div>
          <span class="result-count">{{ filteredConcerts.length }} encontrados</span>
        </div>

        <div v-if="isOffline" class="status-note"><span class="status-dot"></span> La API no está disponible · revisa el backend y vuelve a intentarlo</div>
          <div v-if="actionError" class="status-note"><span class="status-dot"></span> {{ actionError }}</div>
        <div v-if="isLoading" class="loading-state">Buscando tu próxima noche...</div>
        <div v-else-if="!filteredConcerts.length" class="empty-state">No hay conciertos que coincidan con tu búsqueda.</div>
        <div v-else class="concert-list">
          <article v-for="concert in filteredConcerts" :key="concert.id" class="concert-row">
            <div class="date-block"><strong>{{ dayNumber(concert.startDate) }}</strong><span>{{ monthName(concert.startDate) }}</span></div>
            <div class="concert-info"><span class="concert-date">{{ formatDate(concert.startDate) }} · {{ formatTime(concert.startDate) }}</span><h3>{{ concert.name }}</h3><p>{{ concert.venue }}<span v-if="concert.city"> · {{ concert.city }}</span></p></div>
            <span class="genre-tag">LIVE</span>
            <a v-if="concert.ticketUrl && concert.ticketUrl !== '#'" class="ticket-link" :href="concert.ticketUrl" target="_blank" rel="noreferrer">Entradas <span>↗</span></a>
            <span v-else class="ticket-link muted">Próximamente</span>
            <div v-if="concert.artists.length" class="row-actions">
              <button class="action-button" aria-label="Abrir acciones del artista" :aria-expanded="artistMenuConcertId === concert.id" @click="toggleArtistMenu(concert.id)">⋯</button>
              <div v-if="artistMenuConcertId === concert.id" class="artist-actions-menu">
                <template v-for="artist in concert.artists" :key="artist.id">
                  <button v-if="!artist.isFollowed" @click="updateArtistFollow(artist, true)">Seguir {{ artist.name }}</button>
                  <button v-else @click="updateArtistFollow(artist, false)">Dejar de seguir {{ artist.name }}</button>
                </template>
              </div>
            </div>
          </article>
        </div>
      </section>

      <section v-if="nextConcert" class="spotlight-section">
        <div class="spotlight-label"><span class="pulse-dot"></span> DESTACADO DE LA SEMANA</div>
        <div class="spotlight-content"><div><p class="spotlight-city">{{ nextConcert.city }} · {{ nextConcert.venue }}</p><h2>{{ nextConcert.name }}</h2><p class="spotlight-description">{{ nextConcert.description }}</p></div><div class="spotlight-date"><span>{{ formatDate(nextConcert.startDate) }}</span><strong>{{ formatTime(nextConcert.startDate) }}</strong></div></div>
      </section>
    </main>

    <div v-if="authPanelOpen" class="auth-overlay" @click.self="authPanelOpen = false">
      <section class="auth-panel" aria-label="Cuenta de usuario">
        <button class="modal-close" aria-label="Cerrar" @click="authPanelOpen = false">×</button>
        <template v-if="!currentUser">
          <span class="section-kicker">CONCERT RADAR / CUENTA</span>
          <h2>{{ authMode === 'login' ? 'Volver a tus noches.' : 'Crea tu cuenta.' }}</h2>
          <form class="auth-form" @submit.prevent="submitAuth">
            <label v-if="authMode === 'register'">Nombre<input v-model="authForm.name" required minlength="2" autocomplete="name" /></label>
            <label>Email<input v-model="authForm.email" required type="email" autocomplete="email" /></label>
            <label>Contraseña<input v-model="authForm.password" required type="password" minlength="8" autocomplete="current-password" /></label>
            <p v-if="authError" class="form-error">{{ authError }}</p>
            <button class="primary-button" type="submit" :disabled="authLoading">{{ authMode === 'login' ? 'Entrar' : 'Crear cuenta' }}</button>
          </form>
          <button class="text-button" @click="openAuth(authMode === 'login' ? 'register' : 'login')">{{ authMode === 'login' ? '¿Necesitas una cuenta? Regístrate' : 'Ya tengo una cuenta' }}</button>
        </template>
        <template v-else>
          <span class="section-kicker">MI CUENTA</span>
          <h2>{{ currentUser.name }}</h2>
          <form class="auth-form" @submit.prevent="updateProfile">
            <label>Nombre<input v-model="profileForm.name" required minlength="2" autocomplete="name" /></label>
            <label>Email<input :value="currentUser.email" type="email" disabled /></label>
            <button class="primary-button" type="submit">Guardar nombre</button>
          </form>
          <form class="auth-form account-section" @submit.prevent="changePassword">
            <h3>Cambiar contraseña</h3>
            <label>Contraseña actual<input v-model="passwordForm.currentPassword" required type="password" autocomplete="current-password" /></label>
            <label>Nueva contraseña<input v-model="passwordForm.newPassword" required type="password" minlength="8" autocomplete="new-password" /></label>
            <button class="secondary-button" type="submit">Actualizar contraseña</button>
          </form>
          <p v-if="accountMessage" class="form-message">{{ accountMessage }}</p>
          <p v-if="accountError" class="form-error">{{ accountError }}</p>
          <div class="account-actions"><button class="text-button" @click="authPanelOpen = false">Cerrar</button><button class="danger-button" @click="deleteAccount">Eliminar cuenta</button></div>
        </template>
      </section>
    </div>

    <footer><span>© 2025 ConcertRadar</span><span>Hecho para noches con buena música <b>✦</b></span></footer>
  </div>
</template>
