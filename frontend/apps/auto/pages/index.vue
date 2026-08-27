<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import DetailGrid from '@zelo/ui/components/shadcn/DetailGrid.vue'
import SButton from '@zelo/ui/components/shadcn/Button.vue'
import SCard from '@zelo/ui/components/shadcn/Card.vue'
import SInput from '@zelo/ui/components/shadcn/Input.vue'
import SAvatar from '@zelo/ui/components/shadcn/Avatar.vue'

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
  <div class="auto-page">
    <PageHeader
      :title="fullName(selected)"
      :subtitle="`ID: ${selected.id}`"
      :avatar-name="selected.driver"
    />

    <div class="vehicle-details">
      <SCard>
        <div class="card-content">
          <h3>Informações do Veículo</h3>
          <div class="info-grid">
            <div class="info-item">
              <span class="label">VIN</span>
              <span class="value">{{ selected.vin }}</span>
            </div>
            <div class="info-item">
              <span class="label">Matrícula</span>
              <span class="value">{{ selected.plate }}</span>
            </div>
            <div class="info-item">
              <span class="label">Data de Registo</span>
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
              <span class="label">Renovação Seguro</span>
              <span class="value">{{ selected.insuranceRenewal }}</span>
            </div>
            <div class="info-item">
              <span class="label">Próxima Inspeção</span>
              <span class="value">{{ selected.nextInspection }}</span>
            </div>
          </div>
        </div>
      </SCard>
    </div>

    <div class="vehicle-grid">
      <SCard>
        <div class="card-content">
          <h3>Manutenção</h3>
          <div class="maintenance-list">
            <div v-for="entry in selected.maintenances" :key="entry.date" class="maintenance-item">
              <div class="maintenance-header">
                <span class="date">{{ entry.date }}</span>
                <span class="type">{{ entry.type }}</span>
              </div>
              <p class="description">{{ entry.description }}</p>
              <div class="maintenance-details">
                <span>Oficina: {{ entry.workshop }}</span>
                <span>Quilómetros: {{ entry.odometer }}</span>
                <span>Custo: {{ entry.cost }} €</span>
              </div>
            </div>
          </div>
        </div>
      </SCard>

      <SCard>
        <div class="card-content">
          <h3>Estatísticas</h3>
          <div class="stats-list">
            <div class="stat-item">
              <span class="label">Quilómetros (últimos 30 dias)</span>
              <span class="value">{{ formatKms(selected.stats.kmsLastMonth) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">Média por Dia</span>
              <span class="value">{{ selected.stats.avgKmPerDay.toFixed(1) }} km</span>
            </div>
            <div class="stat-item">
              <span class="label">Consumo Médio</span>
              <span class="value">{{ formatConsumption(selected.stats.avgConsumption) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">Custos Manutenção (últimos 30 dias)</span>
              <span class="value">{{ formatCost(selected.stats.maintenanceCostLastMonth) }}</span>
            </div>
          </div>
        </div>
      </SCard>
    </div>

    <div class="vehicle-list">
      <h3>Selecionar Veículo</h3>
      <div class="vehicles">
        <div
          v-for="group in visibleGroups"
          :key="group.label"
          class="vehicle-group"
        >
          <h4>{{ group.label }}</h4>
          <button
            v-for="vehicle in group.items"
            :key="vehicle.id"
            :class="['vehicle-item', { active: vehicle.id === selectedId }]"
            @click="selectedId = vehicle.id"
          >
            <span v-if="logoFor(vehicle)" class="logo">{{ logoFor(vehicle) }}</span>
            <div class="vehicle-info">
              <span class="name">{{ fullName(vehicle) }}</span>
              <span class="plate">{{ vehicle.plate }}</span>
            </div>
          </button>
        </div>

        <p v-if="visibleGroups.length === 0" class="empty">
          Nenhum veículo corresponde a "{{ query }}".
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.auto-page {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  padding: 2rem;
}

.vehicle-details {
  width: 100%;
}

.card-content {
  padding: 1.5rem;
}

.card-content h3 {
  margin: 0 0 1rem 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-item .label {
  font-size: 0.875rem;
  color: #666;
  font-weight: 500;
}

.info-item .value {
  font-size: 1rem;
  color: #333;
}

.vehicle-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
}

.maintenance-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.maintenance-item {
  padding: 1rem;
  border: 1px solid #eee;
  border-radius: 0.5rem;
}

.maintenance-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.maintenance-header .date {
  font-weight: 500;
}

.maintenance-header .type {
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
  background: #f0f0f0;
  border-radius: 0.25rem;
}

.maintenance-item .description {
  margin: 0 0 0.5rem 0;
  font-size: 0.95rem;
}

.maintenance-details {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.875rem;
  color: #666;
}

.stats-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
}

.stat-item .label {
  font-size: 0.95rem;
}

.stat-item .value {
  font-weight: 600;
  font-size: 1.125rem;
}

.vehicle-list {
  width: 100%;
}

.vehicle-list h3 {
  margin: 0 0 1rem 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.vehicle-group h4 {
  margin: 1rem 0 0.5rem 0;
  font-size: 0.95rem;
  color: #666;
  text-transform: uppercase;
}

.vehicles {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.vehicle-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  border: 2px solid transparent;
  border-radius: 0.5rem;
  background: #f9f9f9;
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: left;
}

.vehicle-item:hover {
  background: #f0f0f0;
  border-color: #ddd;
}

.vehicle-item.active {
  background: #e8e8e8;
  border-color: #333;
}

.vehicle-item .logo {
  flex: none;
  font-size: 1.5rem;
  font-weight: 600;
}

.vehicle-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.vehicle-info .name {
  font-weight: 500;
}

.vehicle-info .plate {
  font-size: 0.875rem;
  color: #999;
}

.empty {
  padding: 2rem;
  text-align: center;
  color: #999;
}
</style>
