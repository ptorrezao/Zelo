<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'

const {
  visibleGroups,
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
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2 class="sidebar-title">Veículos</h2>
      </div>

      <div class="sidebar-content">
        <div v-for="group in visibleGroups" :key="group.label" class="vehicle-group">
          <h3 class="group-label">{{ group.label }}</h3>
          <div class="group-items">
            <button
              v-for="vehicle in group.items"
              :key="vehicle.id"
              :class="['vehicle-btn', { active: vehicle.id === selectedId }]"
              @click="selectedId = vehicle.id"
            >
              <div class="vehicle-icon">{{ logoFor(vehicle)?.substring(0, 1) || 'V' }}</div>
              <div class="vehicle-info">
                <span class="name">{{ fullName(vehicle) }}</span>
                <span class="plate">{{ vehicle.plate }}</span>
              </div>
            </button>
          </div>
        </div>
      </div>

      <div class="sidebar-footer">
        <button class="add-btn">+ Adicionar veículo</button>
      </div>
    </aside>

    <main class="content">
      <div class="page-header">
        <div class="header-avatar">{{ selected?.driver?.substring(0, 1) || 'PT' }}</div>
        <div class="header-text">
          <h1 class="header-title">{{ fullName(selected) }}</h1>
          <p class="header-subtitle">ID: {{ selected?.id }}</p>
        </div>
      </div>

      <div class="content-grid">
        <div class="info-section">
          <div class="card">
            <h2 class="card-title">{{ fullName(selected) }}</h2>

            <div class="info-grid">
              <div class="info-item">
                <span class="label">VIN</span>
                <span class="value">{{ selected?.vin }}</span>
              </div>
              <div class="info-item">
                <span class="label">Data de matrícula</span>
                <span class="value">{{ selected?.registered }}</span>
              </div>
              <div class="info-item">
                <span class="label">Quilómetros</span>
                <span class="value">{{ selected?.odometer }}</span>
              </div>
              <div class="info-item">
                <span class="label">Seguradora</span>
                <span class="value">{{ selected?.insurer }}</span>
              </div>
              <div class="info-item">
                <span class="label">Renovação do seguro</span>
                <span class="value">{{ selected?.insuranceRenewal }}</span>
              </div>
              <div class="info-item">
                <span class="label">Próxima inspeção</span>
                <span class="value">{{ selected?.nextInspection }}</span>
              </div>
            </div>

            <div class="plate-section">
              <div class="license-plate">{{ selected?.plate }}</div>
              <a href="#documentos" class="docs-link">Documentos</a>
            </div>

            <div class="photo-container">
              <img v-if="photo" :src="photo" :alt="fullName(selected)" class="vehicle-photo" />
            </div>
          </div>
        </div>

        <div class="stats-section">
          <div class="card">
            <div class="card-header">
              <h3 class="card-title">Manutenção</h3>
              <button class="card-action">+ Adicionar</button>
            </div>

            <div class="timeline">
              <div v-for="entry in selected?.maintenances" :key="entry.date" class="timeline-item">
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
          </div>

          <div class="card">
            <h3 class="card-title">Estatísticas do carro</h3>

            <div class="stats-grid">
              <div class="stat">
                <span class="label">Quilómetros (últimos 30 dias)</span>
                <span class="value">{{ formatKms(selected?.stats?.kmsLastMonth) }}</span>
              </div>
              <div class="stat">
                <span class="label">Média/dia</span>
                <span class="value">{{ selected?.stats?.avgKmPerDay?.toFixed(1) }} km</span>
              </div>
              <div class="stat">
                <span class="label">Consumo médio</span>
                <span class="value">{{ formatConsumption(selected?.stats?.avgConsumption) }}</span>
              </div>
              <div class="stat">
                <span class="label">Custos manutenção (últimos 30 dias)</span>
                <span class="value">{{ formatCost(selected?.stats?.maintenanceCostLastMonth) }}</span>
              </div>
            </div>

            <div class="chart-section">
              <p class="chart-title">Quilómetros por mês</p>
              <div class="chart">
                <div v-for="(km, idx) in selected?.stats?.monthlyKms" :key="idx" class="bar" :style="{ height: (km / 150) + '%' }"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>
