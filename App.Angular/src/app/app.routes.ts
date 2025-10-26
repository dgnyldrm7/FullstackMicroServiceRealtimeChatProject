import { Routes } from '@angular/router';
import { ChatComponent } from './components/chat/chat.component';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
    {
      path: '',
      canActivate: [AuthGuard],
      component: HomeComponent
    },
    {
      path:'chat',
      component: ChatComponent
    },
    {
      path:'login',
      component: LoginComponent
    }
];
