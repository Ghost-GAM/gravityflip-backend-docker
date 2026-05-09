import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // ── CORS habilitado para que cualquier cliente se pueda conectar ──────────
  app.enableCors({
    origin: '*',
    methods: 'GET',
  });

  const PUERTO = 3000;
  await app.listen(PUERTO);

  console.log(`╔═══════════════════════════════════════════════════════════╗`);
  console.log(`║   GravityFlip NestJS API (Solo lectura)                   ║`);
  console.log(`║   Corriendo en: http://localhost:${PUERTO}                     ║`);
  console.log(`║   Lee de PostgreSQL (replica)                             ║`);
  console.log(`║                                                           ║`);
  console.log(`║   Endpoints disponibles:                                  ║`);
  console.log(`║   GET /api/resultado/tabla                                ║`);
  console.log(`║   GET /api/resultado/estadisticas                         ║`);
  console.log(`╚═══════════════════════════════════════════════════════════╝`);
}
bootstrap();