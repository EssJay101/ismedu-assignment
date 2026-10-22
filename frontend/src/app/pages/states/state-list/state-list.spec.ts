import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StateList } from './state-list';

describe('StateList', () => {
  let component: StateList;
  let fixture: ComponentFixture<StateList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StateList],
    }).compileComponents();

    fixture = TestBed.createComponent(StateList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
