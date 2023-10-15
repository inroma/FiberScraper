import 'leaflet/dist/leaflet.css';
import L, { LatLng, latLngBounds } from "leaflet";
import { LMap, LControl, LControlLayers, LTileLayer, LRectangle } from "vue2-leaflet";
import Vue from 'vue';
import { Action } from 'vuex-class';
import { Component } from 'vue-property-decorator';
import { ToastStoreMethods } from '@/store/ToastStore';
import { ISnackbar, ISnackbarColor } from '@/models/SnackbarInterface';
import AutoRefreshInput from '@/models/AutoRefreshInput';
import AutoRefreshService from '@/services/AutoRefreshService';

@Component({
    components: {
        'l-map': LMap,
        'l-control': LControl,
        'l-tile-layer': LTileLayer,
        'l-rectangle': LRectangle,
        LControlLayers,
    }
})
export default class AutoRefreshView extends Vue {
    //#region Public Properties
    public loading = false;
    public controlOpened = false;
    public map: null | LMap = null;
    public userLocation = new LatLng(45.76, 4.83);
    public zoom = 11;
    public tileProviders = [
        {
          name: 'OpenStreetMap',
          visible: true,
          attribution:
            '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
          url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
        },
        {
            name: 'CartoDBDark',
            visible: false,
            url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd'
        },
        {
          name: 'OpenTopoMap',
          visible: false,
          url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
          attribution:
            'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
        }
    ];
    public bounds!: L.LatLngBounds;
    public maxBounds = latLngBounds([
        [40.3097691, -6.0005669],
        [52.0544363, 11.9582151]
    ]);
    public autoRefreshItems: AutoRefreshInput[] = [];
    public rectangles: L.Rectangle[] = [];
    public newRectangle: L.Rectangle[] = [];

    public get headers() {
        return [
            { text: 'Id', value: 'id', align: 'center', width: '65px' },
            { text: 'Label', value: 'label', align: 'center', width: '220px' },
            { text: 'Coord Long', value: 'coordX', align: 'center' },
            { text: 'Coord Lat', value: 'coordY', align: 'center' },
            { text: 'Taille de la zone', value: 'areaSize', align: 'center', width: '150px' },
            { text: 'Dernier Refresh', value: 'lastRun', align: 'center', width: '170px' },
            { text: 'Activé', value: 'enabled', align: 'center', sortable: false },
            { text: 'Actions', value: 'actions', align: 'center', sortable: false, width: '110px' },
        ]
    }

    //#endregion

    @Action(ToastStoreMethods.CREATE_TOAST_MESSAGE)
    private createToast!: (params: ISnackbar) => void;

    public mounted() {
        // this.centerMapOnLocation();
        this.getAutoRefreshInputs();
    }

    public getAutoRefreshInputs() {
        this.loading = true;
        AutoRefreshService.getAll()
        .then((response) => {
            this.autoRefreshItems = response.data;
            this.rectangles = this.autoRefreshItems.flatMap(item => {
                return this.getRectangleFromInput(item);
            });
            this.rectangles = this.rectangles.concat(this.autoRefreshItems.flatMap(item => {
                return this.getRectangleFromInput(item, true);
            }));
            this.newRectangle = [];
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public createItem() {
        const item = new AutoRefreshInput();
        item.isEditing = true;
        item.coordX = this.userLocation.lng;
        item.coordY = this.userLocation.lat;
        this.autoRefreshItems.push(item);
        this.centerUpdate(this.userLocation);
    }

    public getRectangleFromInput(item: AutoRefreshInput, cityOffset: boolean = false) {
        const latOffset = cityOffset === true ? 0.004457 * .25 : 0.004457;
        const lngOffset = cityOffset === true ? 0.006929 * .25 : 0.006929;
        const bounds = new LatLng(item.coordY, item.coordX);
        const areaSizeDivided = item.areaSize/2;
        const rectangle = new L.Rectangle(
            [[bounds.lat - areaSizeDivided*latOffset, bounds.lng - areaSizeDivided*lngOffset],
                [bounds.lat + areaSizeDivided*latOffset, bounds.lng + areaSizeDivided*lngOffset]],
            {
                color: !item.enabled ? 'grey' : latOffset === 0.004457 ? 'red' : 'blue',
                fillColor: 'lightblue',
                weight: 2,
                dashArray: !item.enabled || item.id === 0 ? "6" : undefined
            });
        return rectangle;
    }
    
    public addItem(item: AutoRefreshInput) {
        this.loading = true;
        AutoRefreshService.add(item)
        .then((response) => {
            this.createToast({ color: ISnackbarColor.Success, message: `${response.data} zone créée avec succès` });
            this.getAutoRefreshInputs();
            this.newRectangle = [];
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }
    
    public updateItem(item: AutoRefreshInput) {
        this.loading = true;
        AutoRefreshService.update(item)
        .then((response) => {
            this.createToast({ color: ISnackbarColor.Success, message: `${response.data} zone mise à jour avec succès` });
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            item.isEditing = false;
            this.loading = false;
        });
    }
    
    public deleteItem(item: AutoRefreshInput) {
        if (item.id === 0) {
            this.removeItem(item);
            if (item.isEditing) {
                this.newRectangle = [];
            }
        } else {
            this.loading = true;
            AutoRefreshService.delete(item.id)
            .then((response) => {
                this.createToast({ color: ISnackbarColor.Success, message: `${response.data} zone supprimée avec succès` });
                this.removeItem(item);
            })
            .catch((errors) => {
                this.createToast({ color: ISnackbarColor.Error, message: errors });
            })
            .finally(() => {
                this.loading = false;
            });
        }
    }

    public removeItem(item: AutoRefreshInput) {
        const index = this.autoRefreshItems.findIndex(x => x === item);
        this.autoRefreshItems.splice(index, 1);
    }

    public boundsUpdated (bounds: L.LatLngBounds) {
        this.bounds = bounds;
    }

    public centerMapOnLocation() {
        navigator.geolocation.getCurrentPosition(
            position => {
                this.userLocation = new LatLng(position.coords.latitude, position.coords.longitude);
            }, 
            error => this.createToast({ message: error.message, color: ISnackbarColor.Error }),
            {
                enableHighAccuracy: true,
                timeout: 10000
            });
    }

    public centerUpdate(center: LatLng) {
        this.userLocation = center;
        if(this.autoRefreshItems.some(x => x.isEditing)) {
            const item = this.autoRefreshItems.find(x => x.isEditing);
            item!.coordX = center.lng;
            item!.coordY = center.lat;
            this.newRectangle = [this.getRectangleFromInput(item!), this.getRectangleFromInput(item!, true)];
        }
    }

    public centerMapOnPoint(point: AutoRefreshInput) {
        this.zoom = 16
        this.userLocation = new LatLng(point.coordY, point.coordX);
        // TODO: Draw square
    }

    public editItem(point: AutoRefreshInput) {
        this.centerMapOnPoint(point);
        this.autoRefreshItems.map(x => x.isEditing = false);
        point.isEditing = true;
        this.centerUpdate(this.userLocation);
    }

    public get mapHeight() {
        switch(this.$vuetify.breakpoint.name) {
            case 'xs':
            case 'md':
                return "50vh";
            default:
                return "75vh";
        }
    }

    /** AreaSizes disponibles pour alimenter le champ */
    public get areaSizes() {
        return [1, 3, 5];
    }
}