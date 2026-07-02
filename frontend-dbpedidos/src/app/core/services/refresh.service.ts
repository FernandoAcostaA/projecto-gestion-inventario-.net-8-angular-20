import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface RefreshEvent {
  entity: string;
  action: string;
}

@Injectable({ providedIn: 'root' })
export class RefreshService {
  private changes = new Subject<RefreshEvent>();
  changes$ = this.changes.asObservable();

  notify(entity: string, action: string) {
    this.changes.next({ entity, action });
  }
}
