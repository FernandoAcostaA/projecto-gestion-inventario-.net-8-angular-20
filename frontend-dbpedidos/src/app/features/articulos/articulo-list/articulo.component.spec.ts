import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ArticuloComponent } from './articulo.component';
import { ArticuloService } from '../../../core/services/articulo.service';
import { CategoriaService } from '../../../core/services/categoria.service';
import { PresentacionService } from '../../../core/services/presentacion.service';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

describe('ArticuloComponent', () => {
  let component: ArticuloComponent;
  let fixture: ComponentFixture<ArticuloComponent>;

  const mockArticuloService = {
    getAll: jasmine.createSpy('getAll').and.returnValue(Promise.resolve([])),
    create: jasmine.createSpy('create').and.returnValue(Promise.resolve({})),
    update: jasmine.createSpy('update').and.returnValue(Promise.resolve({})),
    delete: jasmine.createSpy('delete').and.returnValue(Promise.resolve())
  };

  const mockCategoriaService = {
    getAll: jasmine.createSpy('getAll').and.returnValue(Promise.resolve([]))
  };

  const mockPresentacionService = {
    getAll: jasmine.createSpy('getAll').and.returnValue(Promise.resolve([]))
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      declarations: [ArticuloComponent],
      providers: [
        { provide: ArticuloService, useValue: mockArticuloService },
        { provide: CategoriaService, useValue: mockCategoriaService },
        { provide: PresentacionService, useValue: mockPresentacionService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ArticuloComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería crearse', () => {
    expect(component).toBeTruthy();
  });

  it('debería inicializar el formulario con campos requeridos', () => {
    expect(component.form.contains('codigo')).toBeTrue();
    expect(component.form.contains('nombre')).toBeTrue();
    expect(component.form.contains('descripcion')).toBeTrue();
    expect(component.form.contains('imagen')).toBeTrue();
    expect(component.form.contains('idCategoria')).toBeTrue();
    expect(component.form.contains('idPresentacion')).toBeTrue();
  });

  it('debería llamar a crear cuando no se edita', async () => {
    component.form.setValue({
      codigo: 'A001',
      nombre: 'Artículo Prueba',
      descripcion: '',
      imagen: '',
      idCategoria: 1,
      idPresentacion: 1
    });
    await component.submit();
    expect(mockArticuloService.create).toHaveBeenCalled();
  });

  it('debería llamar a actualizar cuando se edita', async () => {
    component.editandoId = 1;
    component.form.setValue({
      codigo: 'A002',
      nombre: 'Artículo Editado',
      descripcion: 'Test',
      imagen: '',
      idCategoria: 2,
      idPresentacion: 2
    });
    await component.submit();
    expect(mockArticuloService.update).toHaveBeenCalledWith(1, jasmine.anything());
  });

  it('debería llamar a eliminar', async () => {
    spyOn(window, 'confirm').and.returnValue(true);
    await component.eliminar(3);
    expect(mockArticuloService.delete).toHaveBeenCalledWith(3);
  });
});
