<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'

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
  <ZSidePanel>
    <div class="side__search">
      <ZSearchInput v-model="query" placeholder="Procurar..." />
    </div>

    <div class="side__list">
      <ZEntityGroup v-for="group in visibleGroups" :key="group.label" :label="group.label">
        <ZEntityItem
          v-for="vehicle in group.items"
          :key="vehicle.id"
          :title="fullName(vehicle)"
          :subtitle="vehicle.plate"
          :image="logoFor(vehicle)"
          :status="vehicle.status"
          :alert="vehicle.alert"
          :selected="vehicle.id === selectedId"
          @click="selectedId = vehicle.id"
        />
      </ZEntityGroup>

      <p v-if="visibleGroups.length === 0" class="side__empty">
        Nenhum veículo corresponde a "{{ query }}".
      </p>
    </div>

    <div class="side__footer">
      <ZButton variant="primary">+ Adicionar veículo</ZButton>
    </div>
  </ZSidePanel>

  <ZWorkspace>
    <ZPanel>
      <div class="head">
        <ZAvatar :name="selected.driver" size="lg" :online="selected.online" />
        <div>
          <p class="head__name">{{ selected.driver }}</p>
          <p class="head__id">ID: {{ selected.id }}</p>
        </div>
      </div>

      <div class="hero">
        <div class="hero__facts">
          <h2 class="hero__model">{{ fullName(selected) }}</h2>

          <ZStatGroup :columns="2">
            <ZStat class="facts__wide" label="VIN" :value="selected.vin" />
            <ZStat label="Data de matrícula" :value="selected.registered" />
            <ZStat label="Quilómetros" :value="selected.odometer" />
            <ZStat label="Seguradora" :value="selected.insurer" />
            <ZStat label="Renovação do seguro" :value="selected.insuranceRenewal" />
            <ZStat label="Próxima inspeção" :value="selected.nextInspection" />
          </ZStatGroup>

          <div class="hero__plate">
            <LicensePlate :value="selected.plate" />
            <a class="hero__docs" href="#documentos">Documentos</a>
          </div>
        </div>

        <VehiclePhoto :src="photo" :alt="fullName(selected)" />
      </div>
    </ZPanel>

    <div class="columns">
      <ZPanel title="Manutenção">
        <template #action>
          <ZButton size="sm">+ Adicionar</ZButton>
        </template>

        <div class="maintenance__timeline">
          <div v-for="(entry, index) in selected.maintenances" :key="entry.date" class="timeline__event">
            <div class="timeline__marker">
              <div class="timeline__dot" :class="`timeline__dot--${entry.type}`" />
            </div>
            <div class="timeline__content">
              <div class="timeline__header">
                <p class="timeline__date">{{ entry.date }}</p>
                <span class="timeline__badge" :class="`timeline__badge--${entry.type}`">{{ entry.type }}</span>
              </div>
              <p class="timeline__title">{{ entry.description }}</p>
              <div class="timeline__details">
                <div class="timeline__detail">
                  <span class="timeline__label">Oficina</span>
                  <p class="timeline__value">{{ entry.workshop }}</p>
                </div>
                <div class="timeline__detail">
                  <span class="timeline__label">Quilómetros</span>
                  <p class="timeline__value">{{ entry.odometer }}</p>
                </div>
                <div class="timeline__detail">
                  <span class="timeline__label">Custo</span>
                  <p class="timeline__value">{{ entry.cost }} €</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </ZPanel>

      <ZPanel title="Estatísticas do carro">
        <ZStatGroup :columns="2">
          <ZStat label="Quilómetros (últimos 30 dias)" :value="formatKms(selected.stats.kmsLastMonth)" />
          <ZStat label="Média/dia" :value="selected.stats.avgKmPerDay.toFixed(1) + ' km'" />
          <ZStat label="Consumo médio" :value="formatConsumption(selected.stats.avgConsumption)" />
          <ZStat label="Custos manutenção (últimos 30 dias)" :value="formatCost(selected.stats.maintenanceCostLastMonth)" />
        </ZStatGroup>

        <div class="stats__chart">
          <ZSectionLabel>Quilómetros por mês</ZSectionLabel>
          <ZBarChart
            :data="selected.stats.monthlyKms"
            value-label="Quilómetros"
            reference-label=""
            :show-reference="false"
          />
        </div>
      </ZPanel>
    </div>
  </ZWorkspace>
</template>

<style scoped>
.side__search {
  border-bottom: var(--z-border-subtle);
}

.side__list {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding-bottom: var(--z-space-2);
}

.side__empty {
  padding: var(--z-space-4);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.side__footer {
  display: grid;
  padding: var(--z-space-3);
  border-top: var(--z-border-subtle);
}

.head {
  display: flex;
  align-items: center;
  gap: var(--z-space-3);
}

.head__name {
  margin: 0;
  font-weight: 600;
}

.head__id {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-6);
  flex-wrap: wrap;
  margin-top: var(--z-space-6);
  padding: var(--z-space-6);
  background: var(--z-color-surface);
  border-radius: var(--z-radius-lg);
}

.hero__facts {
  flex: 1 1 320px;
  min-width: 0;
}

.facts__wide {
  grid-column: 1 / -1;
}

.hero__model {
  margin: 0 0 var(--z-space-6);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
}

.hero__plate {
  display: flex;
  align-items: center;
  gap: var(--z-space-6);
  margin-top: var(--z-space-6);
}

.hero__docs,
.link {
  color: var(--z-color-text-muted);
  font-size: var(--z-font-size-sm);
  text-decoration-style: dashed;
  text-underline-offset: 3px;
}

.columns {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--z-space-3);
  align-items: start;
}

.maintenance__timeline {
  display: flex;
  flex-direction: column;
  gap: 0;
  margin: 0;
  padding: 0;
  list-style: none;
  position: relative;
}

.maintenance__timeline::before {
  content: '';
  position: absolute;
  left: 15px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--z-color-border);
}

.timeline__event {
  display: flex;
  gap: var(--z-space-3);
  margin-bottom: var(--z-space-4);
  position: relative;
  padding-left: var(--z-space-8);
}

.timeline__marker {
  position: absolute;
  left: 0;
  top: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
}

.timeline__dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: var(--z-color-surface);
  border: 3px solid var(--z-color-border);
}

.timeline__dot--preventiva {
  background: var(--z-series-1);
  border-color: var(--z-series-1);
}

.timeline__dot--corretiva {
  background: var(--z-series-4);
  border-color: var(--z-series-4);
}

.timeline__content {
  flex: 1;
  padding: var(--z-space-3);
  background: var(--z-color-surface);
  border-radius: var(--z-radius);
}

.timeline__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-2);
  margin-bottom: var(--z-space-1);
}

.timeline__date {
  margin: 0;
  font-size: var(--z-font-size-sm);
  font-weight: 600;
}

.timeline__badge {
  padding: 2px 8px;
  font-size: var(--z-font-size-xs);
  font-weight: 600;
  border-radius: var(--z-radius-sm);
  text-transform: capitalize;
  white-space: nowrap;
}

.timeline__badge--preventiva {
  background: var(--z-series-1);
  color: #fff;
}

.timeline__badge--corretiva {
  background: var(--z-series-4);
  color: #fff;
}

.timeline__title {
  margin: 0 0 var(--z-space-2);
  font-size: var(--z-font-size-sm);
  font-weight: 600;
}

.timeline__details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: var(--z-space-3);
}

.timeline__detail {
  display: flex;
  flex-direction: column;
}

.timeline__label {
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-muted);
  margin-bottom: 2px;
}

.timeline__value {
  margin: 0;
  font-size: var(--z-font-size-sm);
  font-weight: 600;
  color: var(--z-color-text);
}

.stats__chart {
  margin-top: var(--z-space-8);
}
</style>
