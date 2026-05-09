import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { ResultadoController } from './resultado/resultado.controller';
import { ResultadoService } from './resultado/resultado.service';

@Module({
  imports: [],
  controllers: [AppController, ResultadoController],
  providers: [AppService, ResultadoService],
})
export class AppModule {}