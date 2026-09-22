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
}

const concerts = ref<Concert[]>([])
const search = ref('')
const selectedCity = ref('Todas')
const isLoading = ref(true)
const isOffline = ref(false)
const activeView = ref('Descubrir')

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

watch(search, () => {
  void loadConcerts()
})

watch(selectedCity, () => {
  void loadConcerts()
})

onMounted(loadConcerts)
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
      <button class="profile-button" aria-label="Abrir perfil">AR</button>
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
        </div>
      </section>

      <section class="content-section">
        <div class="section-heading">
          <div><span class="section-kicker">AGENDA ABIERTA</span><h2>Próximos conciertos</h2></div>
          <span class="result-count">{{ filteredConcerts.length }} encontrados</span>
        </div>

        <div v-if="isOffline" class="status-note"><span class="status-dot"></span> La API no está disponible · revisa el backend y vuelve a intentarlo</div>
        <div v-if="isLoading" class="loading-state">Buscando tu próxima noche...</div>
        <div v-else-if="!filteredConcerts.length" class="empty-state">No hay conciertos que coincidan con tu búsqueda.</div>
        <div v-else class="concert-list">
          <article v-for="concert in filteredConcerts" :key="concert.id" class="concert-row">
            <div class="date-block"><strong>{{ dayNumber(concert.startDate) }}</strong><span>{{ monthName(concert.startDate) }}</span></div>
            <div class="concert-info"><span class="concert-date">{{ formatDate(concert.startDate) }} · {{ formatTime(concert.startDate) }}</span><h3>{{ concert.name }}</h3><p>{{ concert.venue }}<span v-if="concert.city"> · {{ concert.city }}</span></p></div>
            <span class="genre-tag">LIVE</span>
            <a v-if="concert.ticketUrl && concert.ticketUrl !== '#'" class="ticket-link" :href="concert.ticketUrl" target="_blank" rel="noreferrer">Entradas <span>↗</span></a>
            <span v-else class="ticket-link muted">Próximamente</span>
          </article>
        </div>
      </section>

      <section v-if="nextConcert" class="spotlight-section">
        <div class="spotlight-label"><span class="pulse-dot"></span> DESTACADO DE LA SEMANA</div>
        <div class="spotlight-content"><div><p class="spotlight-city">{{ nextConcert.city }} · {{ nextConcert.venue }}</p><h2>{{ nextConcert.name }}</h2><p class="spotlight-description">{{ nextConcert.description }}</p></div><div class="spotlight-date"><span>{{ formatDate(nextConcert.startDate) }}</span><strong>{{ formatTime(nextConcert.startDate) }}</strong></div></div>
      </section>
    </main>

    <footer><span>© 2025 ConcertRadar</span><span>Hecho para noches con buena música <b>✦</b></span></footer>
  </div>
</template>
