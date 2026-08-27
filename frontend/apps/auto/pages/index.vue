<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import ZPageHeader from '@zelo/ui/components/ZPageHeader.vue'
import '../styles/tokens.css'
import '../styles/components.css'
import '../styles/pages/index.css'

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
      <div class="side__list">
        <ZEntityGroup v-for="group in visibleGroups" :key="group.label" :label="group.label">
          <ZEntityItem v-for="vehicle in group.items" :key="vehicle.id" :title="fullName(vehicle)"
            :subtitle="vehicle.plate" :image="logoFor(vehicle)" :status="vehicle.status" :alert="vehicle.alert"
            :selected="vehicle.id === selectedId" @click="selectedId = vehicle.id" />
        </ZEntityGroup>

        <p v-if="visibleGroups.length === 0" class="side__empty">
          Nenhum veículo corresponde a "{{ query }}".
        </p>
      </div>

      <div class="side__footer">
        <ZButton variant="primary" size="sm">+ Adicionar veículo</ZButton>
      </div>
    </ZSidePanel>
 <ZWorkspace>
    <div class="main__content">
      <ZPanel>
        <ZPageHeader :title="selected.driver" :subtitle="`ID: ${selected.id}`" :avatar-name="selected.driver" />
      </ZPanel>
      <ZPanel>
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
            <ZButton variant="primary" size="sm">+ Adicionar manutenção</ZButton>
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
            <ZStat label="Custos manutenção (últimos 30 dias)"
              :value="formatCost(selected.stats.maintenanceCostLastMonth)" />
          </ZStatGroup>

          <div class="stats__chart">
            <ZSectionLabel>Quilómetros por mês</ZSectionLabel>
            <ZBarChart :data="selected.stats.monthlyKms" value-label="Quilómetros" reference-label=""
              :show-reference="false" />
          </div>
        </ZPanel>
      </div>
    </div>
  </ZWorkspace>
</template>
