<?php

use Illuminate\Support\Facades\Route;

Route::view('/microui/auth/login', 'microui.auth.login');
Route::view('/microui/auth/register', 'microui.auth.register');

Route::view('/microui/profile/view', 'microui.profile.view')
    ->name('microui.profile.view');

Route::view('/microui/facility/list', 'microui.facility.list')
    ->name('microui.facility.list');

Route::view('/microui/maintenance/list', 'microui.maintenance.list')
    ->name('microui.maintenance.list');

Route::view('/microui/reservations/list', 'microui.reservations.list')
    ->name('microui.reservations.list');

Route::view('/microui/statistics', 'microui.statistics.index')
    ->name('microui.statistics.index');

Route::redirect('/', '/microui/auth/login');