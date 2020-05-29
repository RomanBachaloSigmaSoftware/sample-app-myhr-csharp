import { Component, OnInit } from '@angular/core'
import { ActionsService } from '../shared/actions.service'
import { DocumentType } from '../shared/document-type.enum'

import { Router } from '@angular/router'

@Component({
  selector: 'app-actions',
  templateUrl: './actions.component.html'
})
export class ActionsComponent implements OnInit {
  public documentType = DocumentType;

  constructor (private actionServise: ActionsService, private router: Router) {}

  ngOnInit (): void {}

  sendEnvelope (type: DocumentType) {
    this.actionServise.sendEnvelope(type, null).subscribe((payload) => {
      window.location.href = payload.redirectUrl
    })
  }

  sendTimeCard () {
    this.router.navigate(['/employee/timecard'])
  }
}
