import { FiberPoint, FiberResponseModel } from '@/models/FiberResponseModel';
import axios from 'axios';
import 'leaflet/dist/leaflet.css';
import L, { LatLng } from "leaflet";
import { LMap, LTileLayer, LMarker, LPopup, LControl, LControlLayers } from "vue2-leaflet";
import Vue from 'vue';
import { Mutation } from 'vuex-class';
import { Component } from 'vue-property-decorator';
import { ToastStoreMethods } from '@/store/ToastStore';

@Component({
    components: {
        'l-map': LMap,
        'l-tile-layer': LTileLayer,
        'l-marker': LMarker,
        'l-popup': LPopup,
        'l-control': LControl,
        LControlLayers
    }
})
export default class FiberMapVue extends Vue {
    public loading = false;
    public map: null | LMap = null;
    public userLocation = new LatLng(45.76, 4.83);
    public mapCenter: LatLng = this.userLocation;
    public fibers: null | FiberResponseModel = null;
    public zoom = 11;
    public tileProviders = [
        {
          name: 'OpenStreetMap',
          visible: true,
          attribution:
            '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
          url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
        },
        {
            name: 'CartoDBDark',
            visible: false,
            url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd',
            maxZoom: 20
        },
        {
          name: 'OpenTopoMap',
          visible: false,
          url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
          attribution:
            'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
        }
    ];
    private openedMarker: null | LMarker = null;

    public defaultIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: ''
    });
    public redIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'red-icon',
    });
    public greenIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'green-icon'
    });
    public purpleIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'purple-icon',
    });
    public brownIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'brown-icon',
    });
    public blackIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'black-icon',
    });
    public blueInvertedIcon = new L.Icon.Default({
        iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
        iconUrl: require('leaflet/dist/images/marker-icon.png'),
        shadowUrl: '',
        className: 'blue-inverted-icon',
    });

    public icons = [{code: "ELLIGIBLE", title: "Élligible", icon: this.greenIcon},
        {code: "EN_COURS_IMMEUBLE", title: "Déploiement Immeuble", icon: this.purpleIcon},
        {code: "TERMINE_QUARTIER", title: "Quartier Terminé", icon: this.blueInvertedIcon},
        {code: "EN_COURS_QUARTIER", title: "Déploiement Quartier", icon: this.defaultIcon},
        {code: "PREVU_QUARTIER", title: "Quartier Programmé", icon: this.brownIcon},
        {code: '', title: "Aucune donnée", icon: this.redIcon},
        {code: "*", title: "Statut inconnu", icon: this.blackIcon}];

    public get headers() {
        return [
            { text: 'Adresse', value: 'address.libAdresse' },
            { text: 'Éligibilité FTTH', value: 'eligibilitesFtth' },
            { text: 'Coord Lat', value: 'address.bestCoords.coord.x' },
            { text: 'Coord Long', value: 'address.bestCoords.coord.y' },
        ]
    }

    public created() {
        // fetch the data when the view is created and the data is
        // already being observed
        // this.getFibers();
        this.$store.commit(ToastStoreMethods.CREATE_TOAST_MESSAGE, {show: true, text:"test", color: "secondary", duration: 1000});
    }

    @Mutation(ToastStoreMethods.CREATE_TOAST_MESSAGE) private createToast!: () => void;

    public refreshFibers() {
        this.loading = true;
        axios.get<FiberResponseModel>('https://localhost:5001/api/fiber/RefreshFibers',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.mapCenter.lat, coordY: this.mapCenter.lng }})
        .then((response) => {
            this.fibers = response.data;
        })
        .catch((errors) => {
            //this.createToast();
            console.log(errors);
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public getFibers() {
        this.loading = true;
        axios.get<FiberResponseModel>('https://localhost:5001/api/fiber/GetLiveDataFibers',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.mapCenter.lat, coordY: this.mapCenter.lng }})
        .then((response) => {
            this.fibers = response.data;
        })
        .catch((errors) => {
            console.log(errors);
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public centerUpdate(center: LatLng) {
        this.mapCenter = center;
    }

    public clearData() {
        this.fibers = null;
        this.openedMarker?.mapObject.closePopup();
    }

    public centerMapOnPoint(point: FiberPoint) {
        this.userLocation = new LatLng(point.address.bestCoords.coord.y, point.address.bestCoords.coord.x);
        const marker = this.$refs['marker_'+point.address.signature] as LMarker[];
        this.openedMarker = marker[0];
        this.openedMarker.mapObject.openPopup();
    }

    public getIcon(fiber: FiberPoint): L.Icon.Default {
        if (fiber.eligibilitesFtth.length > 0) {
            const icon = this.icons.filter(a => a.code === fiber.eligibilitesFtth[0].etapeFtth)[0]?.icon;
            if (icon === undefined) {
                return this.blackIcon;
            }
            return icon;
        }
        return this.redIcon;
    }

    public isEligibleToFTTH(item: FiberPoint) {
        return item.eligibilitesFtth?.some(a => a.etapeFtth === 'ELLIGIBLE') ? 'Oui' : 'Non';
    }

    public getEtapeDeploiement(item: FiberPoint) {
        if (item.eligibilitesFtth.length > 0) {
            return item.eligibilitesFtth[0].etapeFtth;
        }
        return '';
    }
}