<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
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
  <div class="auto-layout">
    <!-- Sidebar: Vehicle List -->
    <aside class="sidebar">
      <div class="sidebar-content">
        <h3 class="sidebar-title">Veículos</h3>

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
              <div class="vehicle-info">
                <span class="name">{{ fullName(vehicle) }}</span>
                <span class="plate">{{ vehicle.plate }}</span>
              </div>
            </button>
          </div>
        </div>

        <p v-if="visibleGroups.length === 0" class="empty">
          Nenhum veículo encontrado
        </p>
      </div>

      <div class="sidebar-footer">
        <SButton variant="default" size="sm" class="add-btn">+ Adicionar veículo</SButton>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="main-content">
      <!-- Header -->
      <div class="header-section">
        <PageHeader
          :title="fullName(selected)"
          :subtitle="`ID: ${selected.id}`"
          :avatar-name="selected.driver"
        />
      </div>

      <!-- Vehicle Details Grid -->
      <div class="content-grid">
        <!-- Left: Vehicle Info & Photo -->
        <div class="info-column">
          <SCard class="vehicle-card">
            <div class="card-header">
              <h2>{{ fullName(selected) }}</h2>
            </div>

            <div class="info-grid">
              <div class="info-item">
                <span class="label">VIN</span>
                <span class="value">{{ selected.vin }}</span>
              </div>
              <div class="info-item">
                <span class="label">Data de matrícula</span>
                <span class="value">{{ selected.registered }}</span>
              </div>
              <div class="info-item">
                <span class="label">Quilómetros</span>
                <span class="value">{{ selected.odometer }}</span>
              </div>
              <div class="info-item">
                <span class="label">Seguradora</span>
                <span class="value">{{ selected.insurer }}</span>
              </div>
              <div class="info-item">
                <span class="label">Renovação do seguro</span>
                <span class="value">{{ selected.insuranceRenewal }}</span>
              </div>
              <div class="info-item">
                <span class="label">Próxima inspeção</span>
                <span class="value">{{ selected.nextInspection }}</span>
              </div>
            </div>

            <!-- Plate & Docs -->
            <div class="plate-section">
              <div class="license-plate">{{ selected.plate }}</div>
              <a href="#documentos" class="docs-link">Documentos</a>
            </div>

            <!-- Photo -->
            <div class="photo-container">
              <img v-if="photo" :src="photo" :alt="fullName(selected)" class="vehicle-photo" />
            </div>
          </SCard>
        </div>

        <!-- Right: Maintenance & Stats -->
        <div class="stats-column">
          <!-- Maintenance -->
          <SCard class="maintenance-card">
            <div class="card-header">
              <h3>Manutenção</h3>
              <SButton variant="default" size="sm">+ Adicionar</SButton>
            </div>

            <div class="timeline">
              <div v-for="entry in selected.maintenances" :key="entry.date" class="timeline-item">
                <div class="timeline-marker">
                  <div :class="['dot', `dot-${entry.type}`]"></div>
                </div>
                <div class="timeline-content">
                  <div class="entry-header">
                    <span class="date">{{ entry.date }}</span>
                    <span :class="['badge', `badge-${entry.type}`]">{{ entry.type }}</span>
                  </div>
                  <p class="description">{{ entry.description }}</p>
                  <div class="entry-details">
                    <span>Oficina: {{ entry.workshop }}</span>
                    <span>Quilómetros: {{ entry.odometer }}</span>
                    <span>Custo: {{ entry.cost }} €</span>
                  </div>
                </div>
              </div>
            </div>
          </SCard>

          <!-- Statistics -->
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

            <div class="chart-section">
              <p class="chart-title">Quilómetros por mês</p>
              <div class="chart">
                <div v-for="(km, idx) in selected.stats.monthlyKms" :key="idx" class="bar" :style="{ height: (km / 150) + '%' }"></div>
              </div>
            </div>
          </SCard>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.auto-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 1.5rem;
  padding: 1.5rem;
  background: #f5f5f5;
  min-height: 100vh;
}

@media (max-width: 1024px) {
  .auto-layout {
    grid-template-columns: 1fr;
  }

  .sidebar {
    order: -1;
  }
}

/* Sidebar */
.sidebar {
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  height: fit-content;
  position: sticky;
  top: 1.5rem;
}

.sidebar-content {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.sidebar-title {
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #333;
}

.vehicle-group {
  margin-bottom: 1.5rem;
}

.group-label {
  margin: 0 0 0.5rem 0;
  font-size: 0.75rem;
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
  border: 2px solid transparent;
  border-radius: 0.5rem;
  background: #f9f9f9;
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: left;
}

.vehicle-btn:hover {
  background: #f0f0f0;
  border-color: #ddd;
}

.vehicle-btn.active {
  background: #e8f0ff;
  border-color: #0066cc;
}

.vehicle-icon {
  flex: none;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #e0e0e0;
  border-radius: 0.5rem;
  font-weight: 600;
  font-size: 1rem;
}

.vehicle-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.vehicle-info .name {
  font-weight: 500;
  font-size: 0.9rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.vehicle-info .plate {
  font-size: 0.75rem;
  color: #999;
}

.empty {
  padding: 2rem 0;
  text-align: center;
  color: #999;
  font-size: 0.9rem;
}

.sidebar-footer {
  padding: 1rem;
  border-top: 1px solid #eee;
}

.add-btn {
  width: 100%;
}

/* Main Content */
.main-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.header-section {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 1200px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}

/* Cards */
.vehicle-card,
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

.card-header h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
}

.card-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-item .label {
  font-size: 0.75rem;
  color: #999;
  text-transform: uppercase;
  font-weight: 600;
}

.info-item .value {
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
}

.plate-section {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 0;
  border-top: 1px solid #eee;
  border-bottom: 1px solid #eee;
  margin: 1rem 0;
}

.license-plate {
  padding: 0.5rem 0.75rem;
  background: #003da5;
  color: white;
  font-weight: 600;
  border-radius: 0.25rem;
  font-family: monospace;
  font-size: 0.95rem;
}

.docs-link {
  color: #0066cc;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
}

.docs-link:hover {
  text-decoration: underline;
}

.photo-container {
  display: flex;
  justify-content: center;
  padding-top: 1rem;
}

.vehicle-photo {
  max-width: 100%;
  height: auto;
  max-height: 300px;
  border-radius: 0.5rem;
}

/* Maintenance Timeline */
.timeline {
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

.entry-header {
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

.description {
  margin: 0 0 0.5rem 0;
  font-weight: 500;
  font-size: 0.95rem;
}

.entry-details {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
  color: #666;
}

/* Statistics */
.stats-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.stat {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
}

.stat .label {
  font-size: 0.85rem;
  color: #666;
}

.stat .value {
  font-size: 1.1rem;
  font-weight: 600;
  color: #333;
}

.chart-section {
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.chart-title {
  margin: 0 0 1rem 0;
  font-size: 0.9rem;
  font-weight: 500;
  color: #666;
}

.chart {
  display: flex;
  align-items: flex-end;
  justify-content: space-around;
  gap: 0.4rem;
  height: 120px;
}

.bar {
  flex: 1;
  background: #0066cc;
  border-radius: 0.25rem;
  min-height: 4px;
}
</style>
