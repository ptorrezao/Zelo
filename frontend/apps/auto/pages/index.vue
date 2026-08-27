<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import DetailGrid from '@zelo/ui/components/shadcn/DetailGrid.vue'
import SButton from '@zelo/ui/components/shadcn/Button.vue'
import SCard from '@zelo/ui/components/shadcn/Card.vue'

const {
  visibleGroups,
  query,
  selectedId,
  selected,
  photo,
  fullName,
  logoFor,
  formatConsumption,
  formatKms,
  formatCost,
} = useVehicles()
</script>

<template>
  <div class="auto-container">
    <!-- Header -->
    <div class="header-section">
      <PageHeader
        :title="fullName(selected)"
        :subtitle="`ID: ${selected.id}`"
        :avatar-name="selected.driver"
      />
    </div>

    <!-- Main Grid Layout -->
    <div class="main-grid">
      <!-- Left: Vehicle Info & Photo -->
      <div class="left-column">
        <!-- Vehicle Header Card -->
        <SCard class="vehicle-hero">
          <div class="hero-content">
            <div class="hero-info">
              <h2 class="vehicle-name">{{ fullName(selected) }}</h2>

              <div class="info-section">
                <div class="info-row">
                  <span class="label">VIN</span>
                  <span class="value">{{ selected.vin }}</span>
                </div>
                <div class="info-row">
                  <span class="label">Data de matrícula</span>
                  <span class="value">{{ selected.registered }}</span>
                </div>
                <div class="info-row">
                  <span class="label">Quilómetros</span>
                  <span class="value">{{ selected.odometer }}</span>
                </div>
                <div class="info-row">
                  <span class="label">Seguradora</span>
                  <span class="value">{{ selected.insurer }}</span>
                </div>
                <div class="info-row">
                  <span class="label">Renovação do seguro</span>
                  <span class="value">{{ selected.insuranceRenewal }}</span>
                </div>
                <div class="info-row">
                  <span class="label">Próxima inspeção</span>
                  <span class="value">{{ selected.nextInspection }}</span>
                </div>
              </div>

              <!-- Plate Section -->
              <div class="plate-section">
                <div class="license-plate">{{ selected.plate }}</div>
                <a href="#documentos" class="docs-link">Documentos</a>
              </div>
            </div>

            <!-- Photo Section -->
            <div class="photo-section">
              <img v-if="photo" :src="photo" :alt="fullName(selected)" class="vehicle-photo" />
            </div>
          </div>
        </SCard>
      </div>

      <!-- Right: Maintenance & Stats -->
      <div class="right-column">
        <!-- Maintenance Card -->
        <SCard class="maintenance-card">
          <div class="card-header">
            <h3>Manutenção</h3>
            <SButton variant="default" size="sm">+ Adicionar manutenção</SButton>
          </div>
          <div class="maintenance-timeline">
            <div v-for="entry in selected.maintenances" :key="entry.date" class="timeline-item">
              <div class="timeline-marker">
                <div :class="['dot', `dot-${entry.type}`]"></div>
              </div>
              <div class="timeline-content">
                <div class="timeline-header">
                  <span class="date">{{ entry.date }}</span>
                  <span :class="['badge', `badge-${entry.type}`]">{{ entry.type }}</span>
                </div>
                <p class="title">{{ entry.description }}</p>
                <div class="details">
                  <span>Oficina: {{ entry.workshop }}</span>
                  <span>Quilómetros: {{ entry.odometer }}</span>
                  <span>Custo: {{ entry.cost }} €</span>
                </div>
              </div>
            </div>
          </div>
        </SCard>

        <!-- Statistics Card -->
        <SCard class="stats-card">
          <div class="card-header">
            <h3>Estatísticas do carro</h3>
          </div>
          <div class="stats-grid">
            <div class="stat">
              <span class="label">Quilómetros (últimos 30 dias)</span>
              <span class="value">{{ formatKms(selected.stats.kmsLastMonth) }}</span>
            </div>
            <div class="stat">
              <span class="label">Média/dia</span>
              <span class="value">{{ selected.stats.avgKmPerDay.toFixed(1) }} km</span>
            </div>
            <div class="stat">
              <span class="label">Consumo médio</span>
              <span class="value">{{ formatConsumption(selected.stats.avgConsumption) }}</span>
            </div>
            <div class="stat">
              <span class="label">Custos manutenção (últimos 30 dias)</span>
              <span class="value">{{ formatCost(selected.stats.maintenanceCostLastMonth) }}</span>
            </div>
          </div>
          <div class="chart-placeholder">
            <p>Quilómetros por mês</p>
            <div class="chart">
              <div v-for="(km, idx) in selected.stats.monthlyKms" :key="idx" class="bar" :style="{ height: (km / 150) + '%' }"></div>
            </div>
          </div>
        </SCard>
      </div>
    </div>

    <!-- Vehicle List -->
    <div class="vehicle-list-section">
      <h3 class="section-title">Selecionar Veículo</h3>
      <div class="vehicles-grid">
        <div v-for="group in visibleGroups" :key="group.label" class="vehicle-group">
          <h4 class="group-label">{{ group.label }}</h4>
          <div class="group-items">
            <button
              v-for="vehicle in group.items"
              :key="vehicle.id"
              :class="['vehicle-btn', { active: vehicle.id === selectedId }]"
              @click="selectedId = vehicle.id"
            >
              <div v-if="logoFor(vehicle)" class="vehicle-icon">
                {{ logoFor(vehicle).substring(0, 1) }}
              </div>
              <div class="vehicle-details">
                <span class="vehicle-name">{{ fullName(vehicle) }}</span>
                <span class="vehicle-plate">{{ vehicle.plate }}</span>
              </div>
            </button>
          </div>
        </div>
      </div>

      <p v-if="visibleGroups.length === 0" class="empty-state">
        Nenhum veículo corresponde a "{{ query }}".
      </p>
    </div>

    <!-- Add Vehicle Button -->
    <div class="footer-action">
      <SButton variant="default" size="lg" class="add-vehicle-btn">+ Adicionar veículo</SButton>
    </div>
  </div>
</template>

<style scoped>
.auto-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 2rem;
  background: #f5f5f5;
  min-height: 100vh;
}

.header-section {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.main-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 1200px) {
  .main-grid {
    grid-template-columns: 1fr;
  }
}

/* Left Column */
.left-column {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.vehicle-hero {
  padding: 1.5rem;
}

.hero-content {
  display: flex;
  gap: 2rem;
}

@media (max-width: 900px) {
  .hero-content {
    flex-direction: column;
  }
}

.hero-info {
  flex: 1;
}

.vehicle-name {
  margin: 0 0 1.5rem 0;
  font-size: 1.5rem;
  font-weight: 600;
}

.info-section {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.info-row {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-row .label {
  font-size: 0.75rem;
  color: #999;
  text-transform: uppercase;
  font-weight: 600;
}

.info-row .value {
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
}

.plate-section {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.license-plate {
  padding: 0.5rem 0.75rem;
  background: #003da5;
  color: white;
  font-weight: 600;
  border-radius: 0.25rem;
  font-family: monospace;
}

.docs-link {
  color: #0066cc;
  text-decoration: none;
  font-size: 0.9rem;
}

.docs-link:hover {
  text-decoration: underline;
}

.photo-section {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
}

.vehicle-photo {
  max-width: 250px;
  height: auto;
  border-radius: 0.5rem;
}

/* Right Column */
.right-column {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.maintenance-card,
.stats-card {
  padding: 1.5rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.card-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.maintenance-timeline {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.timeline-item {
  display: flex;
  gap: 1rem;
}

.timeline-marker {
  position: relative;
  padding-top: 0.25rem;
}

.dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 3px solid;
}

.dot-Preventiva {
  background: #0066cc;
  border-color: #0066cc;
}

.dot-Correctiva {
  background: #ff9900;
  border-color: #ff9900;
}

.dot-Inspeção {
  background: #66cc00;
  border-color: #66cc00;
}

.timeline-content {
  flex: 1;
}

.timeline-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.date {
  font-weight: 600;
  font-size: 0.9rem;
}

.badge {
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: white;
}

.badge-Preventiva {
  background: #0066cc;
}

.badge-Correctiva {
  background: #ff9900;
}

.badge-Inspeção {
  background: #66cc00;
}

.title {
  margin: 0 0 0.5rem 0;
  font-weight: 500;
}

.details {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
  color: #666;
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.stat {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
}

.stat .label {
  font-size: 0.8rem;
  color: #999;
}

.stat .value {
  font-size: 1.1rem;
  font-weight: 600;
  color: #333;
}

.chart-placeholder {
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.chart-placeholder p {
  margin: 0 0 0.75rem 0;
  font-size: 0.9rem;
  font-weight: 500;
}

.chart {
  display: flex;
  align-items: flex-end;
  justify-content: space-around;
  gap: 0.5rem;
  height: 120px;
}

.bar {
  flex: 1;
  background: #0066cc;
  border-radius: 0.25rem;
  min-height: 4px;
}

/* Vehicle List */
.vehicle-list-section {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.section-title {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.vehicles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1rem;
}

.vehicle-group h4 {
  margin: 0 0 0.75rem 0;
  font-size: 0.8rem;
  color: #999;
  text-transform: uppercase;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.group-items {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.vehicle-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border: 2px solid #eee;
  border-radius: 0.5rem;
  background: white;
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: left;
}

.vehicle-btn:hover {
  border-color: #ddd;
  background: #f9f9f9;
}

.vehicle-btn.active {
  border-color: #0066cc;
  background: #f0f5ff;
}

.vehicle-icon {
  flex: none;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #e8e8e8;
  border-radius: 0.5rem;
  font-weight: 600;
  font-size: 1.1rem;
}

.vehicle-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.vehicle-name {
  font-weight: 500;
  font-size: 0.95rem;
}

.vehicle-plate {
  font-size: 0.8rem;
  color: #999;
}

.empty-state {
  text-align: center;
  color: #999;
  padding: 2rem;
}

.footer-action {
  display: flex;
  justify-content: center;
  padding: 1rem 0;
}

.add-vehicle-btn {
  width: 100%;
  max-width: 300px;
}
</style>
