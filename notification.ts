export interface INotification {
  name: string;
}

export interface INotificationHandler<TNotification extends INotification> {
  notificationName: string;
  handle(sender: object, event: TNotification): void;
}

export interface IMediator {
  notify<TNotification extends INotification>(
    sender: object,
    event: TNotification
  );
}

export interface IHandlerRegister {
  register<
    TNotification extends INotification,
    TNotificationHandler extends INotificationHandler<TNotification>
  >(
    handler: TNotificationHandler
  );
}

export class Mediator implements IHandlerRegister, IMediator {
  private _handlers: Map<string, Array<any>> = new Map<string, Array<any>>();

  public instance: Mediator = new Mediator();

  register<
    TNotification extends INotification,
    TNotificationHandler extends INotificationHandler<TNotification>
  >(handler: TNotificationHandler) {
    if (handler.notificationName in this._handlers)
      this._handlers[handler.notificationName].push(handler);
    else this._handlers[handler.notificationName] = [handler];
  }

  notify<TNotification extends INotification>(
    sender: object,
    event: TNotification
  ) {
    if (event.name in this._handlers) {
      this._handlers[event.name].forEach((handler) => {
        handler.handle(sender, event);
      });
    }
  }
}

export class AuthenticationNotification implements INotification {
  name: string = "authenticate";

  username: string;
  password: string;
  rememberMe: boolean;
}

export class AuthenticationNotificationHandler
  implements INotificationHandler<AuthenticationNotification>
{
  notificationName: string = "authenticate";
  handle(sender: object, event: AuthenticationNotification): void {
    console.log(event.name + " - " + event.password);
  }
}

export class PontoDeConsumo {}

export class FiltrarPontosDeConsumoNotification implements INotification {
  name: string = "FiltrarPontosDeConsumo";

  // filtros

  onPontosDeConsumoCarregados: (pontosDeConsumo: PontoDeConsumo[]) => void;
}

export class FiltrarPontosDeConsumoHandler
  implements INotificationHandler<FiltrarPontosDeConsumoNotification>
{
  notificationName: string = "FiltrarPontosDeConsumo";

  handle(sender: object, event: FiltrarPontosDeConsumoNotification): void {
    var pontosDeConsumo: PontoDeConsumo[] = [];
    //Carrego os pontos de consumo a partir dos filtros
    event.onPontosDeConsumoCarregados(pontosDeConsumo);
  }
}

export class TelaUmForm {
  private _mediator: IMediator;

  private _pontosDeConsumo: PontoDeConsumo[];

  constructor() {
    let mediator = new Mediator();
    mediator.register(new FiltrarPontosDeConsumoHandler());
    this._mediator = mediator;
  }

  filtrarPontosDeConsumo() {
    let notification = new FiltrarPontosDeConsumoNotification();
    notification.onPontosDeConsumoCarregados = this.renderizarPontosDeConsumo;
    this._mediator.notify(this, notification);
  }

  renderizarPontosDeConsumo(pontosDeConsumo: PontoDeConsumo[]): void {
    this._pontosDeConsumo = pontosDeConsumo;
  }
}

export class TelaDoisForm {
  private _mediator: IMediator;

  private _xablau: PontoDeConsumo[];

  constructor() {
    let mediator = new Mediator();
    mediator.register(new FiltrarPontosDeConsumoHandler());
    this._mediator = mediator;
  }

  filtrarPontosDeConsumo() {
    let notification = new FiltrarPontosDeConsumoNotification();
    notification.onPontosDeConsumoCarregados = this.renderizarPontosDeConsumo;
    this._mediator.notify(this, notification);
  }

  renderizarPontosDeConsumo(pontosDeConsumo: PontoDeConsumo[]): void {
    this._xablau = pontosDeConsumo;
  }
}

export abstract class RequestNotification<TResponse> implements INotification {
  abstract name: string;

  onSuccess: (response: TResponse) => void;
  onFailure: (reason: any) => void;
}

export interface PesquisaFatura {}

export interface Fatura {}

export class PesquisarFaturasNotification extends RequestNotification<
  Fatura[]
> {
  name: string = "PesquisarFaturas";

  filtro: PesquisaFatura;
}

export class FaturaService {
  public carregarFaturas(filtro: PesquisaFatura): Promise<Fatura[]> {
    return new Promise<Fatura[]>((t) => {});
  }
}

export class PesquisarFaturasHandler
  implements INotificationHandler<PesquisarFaturasNotification>
{
  notificationName: string = "PesquisarFaturas";

  constructor(private _faturaService: FaturaService) {}

  handle(sender: object, event: PesquisarFaturasNotification): void {
    this._faturaService
      .carregarFaturas(event.filtro)
      .then(event.onSuccess)
      .catch(event.onFailure);
  }
}

export class TelaFatura {
  private _mediator: IMediator;

  private _filtro: PesquisaFatura = {};

  private _faturas: Fatura[];

  constructor() {
    let mediator = new Mediator();
    mediator.register(new PesquisarFaturasHandler(new FaturaService()));
    this._mediator = mediator;
  }

  pesquisarFaturas() {
    let notification = new PesquisarFaturasNotification();
    notification.filtro = this._filtro;

    notification.onSuccess = this.renderizarFaturas;
    notification.onFailure = this.falhaCarregarTela;

    this._mediator.notify(this, notification);
  }

  renderizarFaturas(faturas: Fatura[]): void {
    this.pararLoading();
    this._faturas = faturas;
  }

  falhaCarregarTela(reason: any): void {
    this.pararLoading();
  }

  iniciarLoading() {}

  pararLoading() {}
}
