import { Controller, Get } from '@nestjs/common';
import { ResultadoService } from './resultado.service';

// ─────────────────────────────────────────────────────────────────────────────
// Controller de la API NestJS
// Solo expone endpoints de LECTURA (GET)
// ─────────────────────────────────────────────────────────────────────────────
@Controller('api/resultado')
export class ResultadoController {
  constructor(private readonly resultadoService: ResultadoService) {}

  // GET /api/resultado/tabla
  @Get('tabla')
  async obtenerTabla() {
    return this.resultadoService.obtenerTopDiez();
  }

  // GET /api/resultado/estadisticas
  @Get('estadisticas')
  async obtenerEstadisticas() {
    return this.resultadoService.obtenerEstadisticas();
  }
}