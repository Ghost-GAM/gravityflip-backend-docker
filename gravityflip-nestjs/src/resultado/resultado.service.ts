import { Injectable, OnModuleDestroy } from '@nestjs/common';
import { Pool } from 'pg';

// ─────────────────────────────────────────────────────────────────────────────
// Servicio que lee de PostgreSQL (la réplica)
// Solo SELECT — esta API es de SOLO LECTURA
// Lee la configuración desde variables de entorno (para Docker)
// ─────────────────────────────────────────────────────────────────────────────
@Injectable()
export class ResultadoService implements OnModuleDestroy {
  private pool: Pool;

  constructor() {
    this.pool = new Pool({
      host: process.env.DB_HOST || 'localhost',
      port: parseInt(process.env.DB_PORT || '5432'),
      database: process.env.DB_NAME || 'gravityflipdb',
      user: process.env.DB_USER || 'postgres',
      password: process.env.DB_PASSWORD || 'GravityFlip2024!',
    });
  }

  async obtenerTopDiez() {
    const query = `
      SELECT id, nombre_jugador, puntaje, muertes,
             tiempo_segundos, fecha_partida
      FROM resultados_partida
      ORDER BY puntaje DESC
      LIMIT 10
    `;

    const result = await this.pool.query(query);

    return result.rows.map((r, i) => ({
      posicion: i + 1,
      nombre: r.nombre_jugador,
      puntaje: r.puntaje,
      muertes: r.muertes,
      tiempo: this.formatearTiempo(r.tiempo_segundos),
      fecha: new Date(r.fecha_partida).toLocaleDateString('es-MX'),
    }));
  }

  async obtenerEstadisticas() {
    const query = `
      SELECT
        COUNT(*)               AS total_jugadores,
        COALESCE(MAX(puntaje), 0)        AS mejor_puntaje,
        COALESCE(AVG(puntaje), 0)::int   AS promedio_puntaje,
        COALESCE(SUM(muertes), 0)        AS total_muertes
      FROM resultados_partida
    `;

    const result = await this.pool.query(query);
    const stats = result.rows[0];

    return {
      totalJugadores: parseInt(stats.total_jugadores),
      mejorPuntaje: parseInt(stats.mejor_puntaje),
      promedioPuntaje: parseInt(stats.promedio_puntaje),
      totalMuertes: parseInt(stats.total_muertes),
      fuente: 'PostgreSQL (replica de solo lectura)',
    };
  }

  private formatearTiempo(segundos: number): string {
    const min = Math.floor(segundos / 60);
    const seg = Math.floor(segundos % 60);
    return `${String(min).padStart(2, '0')}:${String(seg).padStart(2, '0')}`;
  }

  async onModuleDestroy() {
    await this.pool.end();
  }
}