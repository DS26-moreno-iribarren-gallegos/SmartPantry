import { Component, Input, Pipe, PipeTransform } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ListService,
  LocalizationPipe,
  LocalizationService,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '@abp/ng.core';
import { Confirmation, ConfirmationService, ModalComponent } from '@abp/ng.theme.shared';
import { Observable, Subject, of, throwError } from 'rxjs';
import {
  PantryItemDto,
  PantryItemService,
  PantryItemStatus,
  UpdatePantryItemDto,
} from '../proxy/pantry';
import { PantryComponent } from './pantry.component';

@Pipe({
  name: 'abpLocalization',
  standalone: true,
})
class MockLocalizationPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

@Component({
  selector: 'abp-modal',
  standalone: true,
  template: '<ng-content></ng-content>',
})
class MockModalComponent {
  @Input() visible = false;
}

describe('PantryComponent', () => {
  let fixture: ComponentFixture<PantryComponent>;
  let component: PantryComponent;

  let getListHandler: (
    query: PagedAndSortedResultRequestDto
  ) => Observable<PagedResultDto<PantryItemDto>>;
  let updateCalls: Array<{ id: string; dto: UpdatePantryItemDto }>;
  let updateResponse$: Observable<PantryItemDto>;
  let markAsConsumedCalls: string[];
  let deleteCalls: string[];
  let confirmationResult: Confirmation.Status;
  let refreshCount: number;

  const sampleItems: PantryItemDto[] = [
    {
      id: '11111111-1111-1111-1111-111111111111',
      productBarcode: '7790742000105',
      productName: 'Leche Entera 1L',
      status: PantryItemStatus.Available,
      quantity: 3,
      unit: 'litros',
      expirationDate: '2026-12-15T00:00:00Z',
    },
    {
      id: '22222222-2222-2222-2222-222222222222',
      productBarcode: '7790895000997',
      productName: null,
      status: PantryItemStatus.Consumed,
      quantity: null,
      unit: null,
      expirationDate: null,
    },
  ];

  beforeEach(async () => {
    updateCalls = [];
    markAsConsumedCalls = [];
    deleteCalls = [];
    confirmationResult = Confirmation.Status.confirm;
    refreshCount = 0;

    getListHandler = () =>
      of({
        items: sampleItems,
        totalCount: sampleItems.length,
      });

    updateResponse$ = of({
      ...sampleItems[0],
      quantity: 5,
      unit: 'L',
    });

    const mockPantryItemService = {
      getList: (query: PagedAndSortedResultRequestDto) => getListHandler(query),
      update: (id: string, dto: UpdatePantryItemDto) => {
        updateCalls.push({ id, dto });
        return updateResponse$;
      },
      markAsConsumed: (id: string) => {
        markAsConsumedCalls.push(id);
        return of({ ...sampleItems[0], status: PantryItemStatus.Consumed });
      },
      delete: (id: string) => {
        deleteCalls.push(id);
        return of(void 0);
      },
    };

    const mockListService = {
      page: 0,
      maxResultCount: 10,
      hookToQuery: (
        streamCreator: (
          query: PagedAndSortedResultRequestDto
        ) => Observable<PagedResultDto<PantryItemDto>>
      ) => streamCreator({ skipCount: 0, maxResultCount: 10 }),
      get: () => {
        refreshCount++;
      },
    };

    const mockConfirmationService = {
      warn: () => of(confirmationResult),
    };

    const mockLocalizationService = {
      currentLang: 'es',
    };

    await TestBed.configureTestingModule({
      imports: [PantryComponent],
      providers: [
        { provide: PantryItemService, useValue: mockPantryItemService },
        { provide: ConfirmationService, useValue: mockConfirmationService },
        { provide: LocalizationService, useValue: mockLocalizationService },
      ],
    })
      .overrideComponent(PantryComponent, {
        remove: {
          imports: [LocalizationPipe, ModalComponent],
          providers: [ListService],
        },
        add: {
          imports: [MockLocalizationPipe, MockModalComponent],
          providers: [{ provide: ListService, useValue: mockListService }],
        },
      })
      .compileComponents();
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(PantryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('debe mostrar el estado de carga mientras espera la respuesta de la API', () => {
    const pending$ = new Subject<PagedResultDto<PantryItemDto>>();
    getListHandler = () => pending$.asObservable();

    createComponent();

    expect(component.isLoading).toBe(true);
    expect(fixture.nativeElement.querySelector('[data-testid="pantry-loading"]')).not.toBeNull();
  });

  it('debe renderizar la tabla con ítems y textos alternativos para campos vacíos o producto desconocido', () => {
    createComponent();

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(false);
    expect(component.items.length).toBe(2);

    const resultsEl: HTMLElement | null = fixture.nativeElement.querySelector(
      '[data-testid="pantry-results"]'
    );
    expect(resultsEl).not.toBeNull();
    expect(resultsEl?.textContent).toContain('Leche Entera 1L');
    expect(resultsEl?.textContent).toContain('::Pantry:UnknownProduct');
    expect(resultsEl?.textContent).toContain('::Pantry:NotSpecified');
    expect(resultsEl?.textContent).toContain('::Pantry:NoUnit');
    expect(resultsEl?.textContent).toContain('::Pantry:NoExpirationDate');
  });

  it('debe mostrar el estado vacío cuando el usuario no tiene ítems registrados', () => {
    getListHandler = () => of({ items: [], totalCount: 0 });

    createComponent();

    expect(fixture.nativeElement.querySelector('[data-testid="pantry-empty"]')).not.toBeNull();
  });

  it('debe mostrar el estado de error si falla la carga y permitir reintentar', () => {
    getListHandler = () => throwError(() => new Error('Network error'));

    createComponent();

    expect(component.hasError).toBe(true);
    expect(fixture.nativeElement.querySelector('[data-testid="pantry-error"]')).not.toBeNull();

    component.retry();
    expect(refreshCount).toBe(1);
  });

  it('debe filtrar los ítems por estado (Todos, Disponibles, Consumidos)', () => {
    createComponent();

    expect(component.filteredItems.length).toBe(2);

    component.setFilter('available');
    expect(component.filteredItems.length).toBe(1);
    expect(component.filteredItems[0].id).toBe(sampleItems[0].id);

    component.setFilter('consumed');
    expect(component.filteredItems.length).toBe(1);
    expect(component.filteredItems[0].id).toBe(sampleItems[1].id);
  });

  it('debe impedir guardar si la cantidad es negativa y enviar el DTO válido al corregirla', () => {
    createComponent();

    component.openEditModal(sampleItems[0]);
    expect(component.isEditModalOpen).toBe(true);

    component.editForm.patchValue({ quantity: -3 });
    component.saveEdit();

    expect(component.editForm.invalid).toBe(true);
    expect(updateCalls.length).toBe(0);

    component.editForm.patchValue({
      quantity: 5,
      unit: 'L',
      expirationDate: '2026-12-31',
    });
    component.saveEdit();

    expect(updateCalls.length).toBe(1);
    expect(updateCalls[0]).toEqual({
      id: sampleItems[0].id!,
      dto: {
        quantity: 5,
        unit: 'L',
        expirationDate: '2026-12-31T00:00:00.000Z',
      },
    });
    expect(component.isEditModalOpen).toBe(false);
    expect(refreshCount).toBe(1);
  });

  it('debe marcar un ítem disponible como consumido e ignorar los que ya están consumidos', () => {
    createComponent();

    component.markAsConsumed(sampleItems[1]);
    expect(markAsConsumedCalls.length).toBe(0);

    component.markAsConsumed(sampleItems[0]);
    expect(markAsConsumedCalls).toEqual([sampleItems[0].id!]);
    expect(refreshCount).toBe(1);
  });

  it('debe eliminar el ítem solo cuando el usuario confirma el diálogo', () => {
    createComponent();

    confirmationResult = Confirmation.Status.reject;
    component.deleteItem(sampleItems[0]);
    expect(deleteCalls.length).toBe(0);

    confirmationResult = Confirmation.Status.confirm;
    component.deleteItem(sampleItems[0]);
    expect(deleteCalls).toEqual([sampleItems[0].id!]);
    expect(refreshCount).toBe(1);
  });
});
